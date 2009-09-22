/******************************************************************************
  Copyright 2009 dataweb GmbH
  This file is part of the nShape framework.
  nShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  nShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  nShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Dataweb.NShape.Advanced {
	
	public class CommandEventArgs : EventArgs {

		public CommandEventArgs(ICommand command, bool reverted)
			: this() {
			if (command == null) throw new ArgumentNullException("command");
			this.command = command;
			this.reverted = reverted;
		}


		public ICommand Command { 
			get { return this.command; }
			internal set { command = value; }
		}


		public bool Reverted {
			get { return reverted; }
			internal set { reverted = value; }
		}


		internal CommandEventArgs() {
		}


		private ICommand command = null;
		private bool reverted = false;
	}


	public class CommandsEventArgs : EventArgs {

		public CommandsEventArgs(IEnumerable<ICommand> commands, bool reverted) {
			if (commands == null) throw new ArgumentNullException("commands");
			AddRange(commands);
		}


		public IReadOnlyCollection<ICommand> Commands {
			get { return commands; }
		}


		public bool Reverted {
			get { return reverted; }
			internal set { reverted = value; }
		}


		internal CommandsEventArgs() {
		}


		internal void Add(ICommand command){
			commands.Add(command);
		}


		internal void AddRange(IEnumerable<ICommand> commands) {
			this.Clear();
			this.commands.AddRange(commands);
		}


		internal void Clear() {
			commands.Clear();
		}


		private ReadOnlyList<ICommand> commands = new ReadOnlyList<ICommand>();
		private bool reverted;
	}


	/// <summary>
	/// Stores a sequence of commands for undo and redo operations.
	/// </summary>
	public class History {

		/// <summary>
		/// Constructs a new history.
		/// </summary>
		/// <param name="capacity"></param>
		public History() {
			commands = new List<ICommand>(100);
		}


		public event EventHandler<CommandEventArgs> CommandAdded;


		public event EventHandler<CommandEventArgs> CommandExecuted;


		public event EventHandler<CommandsEventArgs> CommandsExecuted;


		/// <summary>
		/// Starts collecting all added commands in an aggregated command.
		/// Aggregated commands can be undone with a single call of Undo()/Redo().
		/// While aggregating commands, the CommandAdded event will not be raised.
		/// </summary>
		public void BeginAggregatingCommands() {
			aggregatingCommands = true;
			aggregatedCommand = new AggregatedCommand();
		}


		/// <summary>
		/// End aggregation of commands and adds the AggregatedCommand to the History. 
		/// Raises a CommandAdded event.
		/// Does not execute the collected commands.
		/// </summary>
		public void EndAggregatingCommands() {
			aggregatingCommands = false;
			AddCommand(aggregatedCommand);
			aggregatedCommand = null;
		}


		/// <summary>
		/// Cancels the aggregation of commands. All collected commands will be undone and not added to the history.
		/// </summary>
		public void CancelAggregatingCommands() {
			if (aggregatingCommands) {
				aggregatingCommands = false;
				aggregatedCommand.Revert();
				aggregatedCommand = null;
			}
		}


		/// <summary>
		/// Returns descriptions of all available redo commands
		/// </summary>
		public IEnumerable<string> GetRedoCommandDescriptions(int count) {
			int stopIdx = currentPosition + count;
			if (stopIdx >= commands.Count) stopIdx = commands.Count - 1;
			for (int i = currentPosition + 1; i <= stopIdx; ++i)
				yield return commands[i].Description;
		}


		/// <summary>
		/// Returns descriptions of all available undo commands
		/// </summary>
		public IEnumerable<string> GetUndoCommandDescriptions(int count) {
			int stopIdx = currentPosition - count + 1;
			if (stopIdx < 0) stopIdx = 0;
			for (int i = currentPosition; i >= stopIdx; --i)
				yield return commands[i].Description;
		}


		/// <summary>
		/// Returns descriptions of all available redo commands
		/// </summary>
		public IEnumerable<string> GetRedoCommandDescriptions() {
			return GetRedoCommandDescriptions(RedoCommandCount);
		}


		/// <summary>
		/// Returns descriptions of all available undo commands
		/// </summary>
		public IEnumerable<string> GetUndoCommandDescriptions() {
			return GetUndoCommandDescriptions(UndoCommandCount);
		}


		/// <summary>
		/// Returns description of the next available redo command
		/// </summary>
		public string GetRedoCommandDescription() {
			ICommand cmd = GetNextCommand();
			if (cmd != null) return cmd.Description;
			else return string.Empty;
		}


		/// <summary>
		/// Returns description of the next available undo command
		/// </summary>
		public string GetUndoCommandDescription() {
			ICommand cmd = GetPreviousCommand();
			if (cmd != null) return cmd.Description;
			else return string.Empty;
		}


		/// <summary>
		/// Returns the number of commands that can be redone.
		/// </summary>
		public int RedoCommandCount {
			get {
				if (currentPosition < 0) return commands.Count;
				else return (commands.Count - 1) - currentPosition;
			}
		}


		/// <summary>
		/// Returns the number of commands that can be undone.
		/// </summary>
		public int UndoCommandCount {
			get { return currentPosition + 1; }
		}


		/// <summary>
		/// Indicates, whether the given command is the next one to undo.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public bool IsNextUndoCommand(ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			return GetPreviousCommand() == command;
		}


		/// <summary>
		/// Undo the current action
		/// </summary>
		public void Undo() {
			ICommand cmd = PerformUndo();
			if (CommandExecuted != null) 
				CommandExecuted(this, GetCommandEventArgs(cmd, true));
		}


		/// <summary>
		/// Undo the given number of commands at once.
		/// </summary>
		/// <param name="commandCount"></param>
		public void Undo(int commandCount) {
			commandsEventArgsBuffer.Clear();
			commandsEventArgsBuffer.Reverted = true;

			for (int i = 0; i < commandCount; ++i) {
				ICommand cmd = PerformUndo();
				commandsEventArgsBuffer.Add(cmd);
			}

			if (CommandsExecuted!=null) CommandsExecuted(this, commandsEventArgsBuffer);
		}


		/// <summary>
		/// Redo the last undone action
		/// </summary>
		public void Redo() {
			ICommand cmd = PerformRedo();
			if (CommandExecuted != null)
				CommandExecuted(this, GetCommandEventArgs(cmd, false));
		}


		/// <summary>
		/// Redo the given number of commands at once.
		/// </summary>
		/// <param name="commandCount"></param>
		public void Redo(int commandCount) {
			commandsEventArgsBuffer.Clear();
			commandsEventArgsBuffer.Reverted = true;

			for (int i = 0; i < commandCount; ++i) {
				ICommand cmd = PerformRedo();
				commandsEventArgsBuffer.Add(cmd);
			}

			if (CommandsExecuted != null) CommandsExecuted(this, commandsEventArgsBuffer);
		}


		/// <summary>
		/// Executes the given command and adds it to the Undo/Redo list
		/// </summary>
		/// <param name="command"></param>
		public void ExecuteAndAddCommand(ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			command.Execute();
			AddCommand(command);
		}
		
		
		/// <summary>
		/// Adds a command to the History. The command will not be executed by this method.
		/// </summary>
		/// <param name="command"></param>
		public void AddCommand(ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			if (aggregatingCommands) {
				Debug.Assert(aggregatedCommand != null);
				aggregatedCommand.Add(command);
			} else {
				int redoPos = currentPosition + 1;
				if (redoPos >= 0 && commands.Count > redoPos)
					commands.RemoveRange(redoPos, commands.Count - redoPos);
				commands.Add(command);
				currentPosition = commands.Count - 1;
				
				if (CommandAdded != null) 
					CommandAdded(this, GetCommandEventArgs(command, false));
				// TODO 2: Remove oldest command, if list grows over its capacity.
			}
		}


		/// <summary>
		/// Clears all commands in the history.
		/// </summary>
		public void Clear() {
			commands.Clear();
			currentPosition = -1;
		}


		// Returns the previous command to undo
		private ICommand GetPreviousCommand() {
			ICommand result = null;
			if (currentPosition >= 0)
				result = commands[currentPosition];
			return result;
		}


		// Returns the next command to redo
		private ICommand GetNextCommand() {
			ICommand result = null;
			if (currentPosition < commands.Count - 1)
				result = commands[currentPosition + 1];
			return result;
		}


		private ICommand PerformUndo() {
			if (aggregatingCommands)
				EndAggregatingCommands();
			ICommand cmd = GetPreviousCommand();
			RevertCommand(cmd);
			return cmd;
		}
		
		
		private ICommand PerformRedo() {
			if (aggregatingCommands)
				EndAggregatingCommands();
			ICommand cmd = GetNextCommand();
			ExecuteCommand(cmd);
			return cmd;
		}
		
		
		private void ExecuteCommand(ICommand command) {
			Debug.Assert(command != null);
			Debug.Assert(commands.Contains(command));
			command.Execute();
			++currentPosition;
		}


		private void RevertCommand(ICommand command) {
			Debug.Assert(command != null);
			Debug.Assert(commands.Contains(command));
			command.Revert();
			--currentPosition;
		}


		private CommandEventArgs GetCommandEventArgs(ICommand command, bool reverted) {
			commandEventArgsBuffer.Command = command;
			commandEventArgsBuffer.Reverted = reverted;
			return commandEventArgsBuffer;
		}


		#region Fields
		// Undo/redo list of commands
		private List<ICommand> commands;		
		private int currentPosition = -1;
		private bool aggregatingCommands = false;
		private AggregatedCommand aggregatedCommand;

		private CommandEventArgs commandEventArgsBuffer = new CommandEventArgs();
		private CommandsEventArgs commandsEventArgsBuffer = new CommandsEventArgs();
		#endregion
	}

}
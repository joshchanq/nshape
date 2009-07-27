using System;
using System.Collections.Generic;
using System.Text;


namespace Dataweb.Utilities {

	#region Simulated Annealing

	/// <summary>
	/// Defines the problem for the simulated annealing algorithm.
	/// </summary>
	public interface ISimulatedAnnealingProblem {
		object FindInitialState();
		object FindNeighbourState(object state);
		object SetState(object state);
		float CalculateEnergy(object state);
	}


	public class SimulatedAnnealingSolver {

		public SimulatedAnnealingSolver() {
		}


		public void Reset(ISimulatedAnnealingProblem problem, float maxEnergy) {
			this.problem = problem;
			this.maxEnergy = maxEnergy;
		}

		public void Run() {
			object state = problem.FindInitialState();
			float energy = problem.CalculateEnergy(state);
			object bestState = state;
			float bestEnergy = energy;
			int loops = 0;
			while (loops < 1000 && energy > bestEnergy) {
				object nextState = problem.FindNeighbourState(state);
				float nextEnergy = problem.CalculateEnergy(nextState);
				if (nextEnergy < bestEnergy) {
					bestState = nextState;
					bestEnergy = nextEnergy;
				}
				if (AcceptanceProbability(energy, nextEnergy, Temperature(loops / 1000)) > random.NextDouble()) {
					state = nextState;
					energy = nextEnergy;
				}
				++loops;
			}
			problem.SetState(bestState);
		}


		private float Temperature(float time) {
			return 1000 * (1.0F - time);
		}


		private float AcceptanceProbability(float currentEnergy, float newEnergy, float temperature) {
			float result;
			if (newEnergy < currentEnergy) result = 1.0F;
			else if (temperature < 0.0001F) result = 0.0F;
			else result = (float)Math.Exp((currentEnergy - newEnergy) / temperature);
			return result;
		}


		private ISimulatedAnnealingProblem problem;

		private float maxEnergy;

		private Random random = new Random();

	}

	#endregion

}

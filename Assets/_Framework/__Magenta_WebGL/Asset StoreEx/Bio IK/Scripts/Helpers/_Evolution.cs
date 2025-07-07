using UnityEngine;
using System.Threading;
using System.Collections.Generic;

namespace BioIK {

	//----------------------------------------------------------------------------------------------------
	//====================================================================================================
	//Memetic Evolutionary Optimisation
	//====================================================================================================
	//----------------------------------------------------------------------------------------------------
	public class _Evolution : Evolution
    {
        private static string LOG_FORMAT = "<color=#DBAE31><b>[_Evolution]</b></color> {0}";

        // protected bool ThreadsRunning = false;
        protected bool _ThreadsRunning
        {
            get
            {
                // In Unity WebGL, multiple threads cannot be used by default.
                Debug.Assert(ThreadsRunning == false);
                return ThreadsRunning;
            }
        }

        // protected Model Model;
        protected _Model _model
        {
            get 
            { 
                return Model as _Model;
            }
        }

        // protected double[] Solution;
        protected double[] _solutions
        {
            get
            {
                return Solution;
            }
        }

        public _Evolution(/*_Model model, int populationSize, int elites, bool useThreading*/)
        {
            /*
            Model = model;
            PopulationSize = populationSize;
            Elites = elites;
            Dimensionality = model.GetDoF();
            UseThreading = useThreading;

            Population = new Individual[PopulationSize];
            Offspring = new Individual[PopulationSize];
            for (int i = 0; i < PopulationSize; i++)
            {
                Population[i] = new Individual(Dimensionality);
                Offspring[i] = new Individual(Dimensionality);
            }

            LowerBounds = new double[Dimensionality];
            UpperBounds = new double[Dimensionality];
            Constrained = new bool[Dimensionality];
            Probabilities = new double[PopulationSize];
            Solution = new double[Dimensionality];

            Models = new Model[Elites];
            Optimisers = new BFGS[Elites];
            Improved = new bool[Elites];
            for (int i = 0; i < Elites; i++)
            {
                int index = i;
                Models[index] = new Model(Model.GetCharacter());
                Optimisers[index] = new BFGS(Dimensionality, x => Models[index].ComputeLoss(x), y => Models[index].ComputeGradient(y, 1e-5));
            }

            if (UseThreading)
            {
                //Start Threads
                ThreadsRunning = true;
                Work = new bool[Elites];
                Handles = new ManualResetEvent[Elites];
                Threads = new Thread[Elites];
                for (int i = 0; i < Elites; i++)
                {
                    int index = i;
                    Work[index] = false;
                    Handles[index] = new ManualResetEvent(true);
                    Threads[index] = new Thread(x => SurviveThread(index));
                    //Threads[index].Start();
                }
            }
            else
            {
                ThreadsRunning = false;
            }
            */
        }

        public _Evolution(_Model model, int populationSize, int elites/*, bool useThreading*/) : base()
        {
            /*
            Model = model;
            PopulationSize = populationSize;
            Elites = elites;
            Dimensionality = model.GetDoF();
            UseThreading = useThreading;

            Population = new Individual[PopulationSize];
            Offspring = new Individual[PopulationSize];
            for (int i = 0; i < PopulationSize; i++)
            {
                Population[i] = new Individual(Dimensionality);
                Offspring[i] = new Individual(Dimensionality);
            }

            LowerBounds = new double[Dimensionality];
            UpperBounds = new double[Dimensionality];
            Constrained = new bool[Dimensionality];
            Probabilities = new double[PopulationSize];
            Solution = new double[Dimensionality];

            Models = new Model[Elites];
            Optimisers = new BFGS[Elites];
            Improved = new bool[Elites];
            for (int i = 0; i < Elites; i++)
            {
                int index = i;
                Models[index] = new Model(Model.GetCharacter());
                Optimisers[index] = new BFGS(Dimensionality, x => Models[index].ComputeLoss(x), y => Models[index].ComputeGradient(y, 1e-5));
            }

            if (UseThreading)
            {
                //Start Threads
                ThreadsRunning = true;
                Work = new bool[Elites];
                Handles = new ManualResetEvent[Elites];
                Threads = new Thread[Elites];
                for (int i = 0; i < Elites; i++)
                {
                    int index = i;
                    Work[index] = false;
                    Handles[index] = new ManualResetEvent(true);
                    Threads[index] = new Thread(x => SurviveThread(index));
                    //Threads[index].Start();
                }
            }
            else
            {
                ThreadsRunning = false;
            }
            */

            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");

            Model = model;
			PopulationSize = populationSize;
			Elites = elites;
			Dimensionality = model.GetDoF();
            // UseThreading = useThreading;
            UseThreading = false; // In Unity WebGL, multiple threads cannot be used by default.

            Population = new Individual[PopulationSize];
			Offspring = new Individual[PopulationSize];
			for(int i=0; i<PopulationSize; i++) 
            {
				Population[i] = new Individual(Dimensionality);
				Offspring[i] = new Individual(Dimensionality);
			}

			LowerBounds = new double[Dimensionality];
			UpperBounds = new double[Dimensionality];
			Constrained = new bool[Dimensionality];
			Probabilities = new double[PopulationSize];
			Solution = new double[Dimensionality];

            Models = new Model[Elites];
            Optimisers = new BFGS[Elites];
            Improved = new bool[Elites];
            for(int i=0; i<Elites; i++) 
            {
                int index = i;
                Models[index] = new _Model(_model.GetCharacter() as _BioIK);
                Optimisers[index] = new BFGS(Dimensionality, x => Models[index].ComputeLoss(x), y => Models[index].ComputeGradient(y, 1e-5));
            }

            Debug.LogFormat(LOG_FORMAT, "Elites : <b>" + Elites + "</b>");
#if false // In Unity WebGL, multiple threads cannot be used by default.
            if (UseThreading == true) 
            {
                //Start Threads
                ThreadsRunning = true;
                Work = new bool[Elites];
                Handles = new ManualResetEvent[Elites];
                Threads = new Thread[Elites];
                for(int i=0; i<Elites; i++) 
                {
                    int index = i;
                    Work[index] = false;
                    Handles[index] = new ManualResetEvent(true);
                    Threads[index] = new Thread(x => SurviveThread(index));
                    //Threads[index].Start();
                }
            } 
            else 
#endif
            {
                ThreadsRunning = false;
            }
		}

        protected override void SurviveThread(int index)
        {
#if true //
            throw new System.NotSupportedException("In Unity WebGL, multiple threads cannot be used by default.");
#else
            while (ThreadsRunning)
            {
                Work[index] = true;

                //Copy elitist survivor
                Individual survivor = Population[index];
                Individual elite = Offspring[index];
                for (int i = 0; i < Dimensionality; i++)
                {
                    elite.Genes[i] = survivor.Genes[i];
                    elite.Momentum[i] = survivor.Momentum[i];
                }

                //Exploit
                double fitness = Models[index].ComputeLoss(elite.Genes);
                Optimisers[index].Minimise(elite.Genes, ref Evolving);
                if (Optimisers[index].Value < fitness)
                {
                    for (int i = 0; i < Dimensionality; i++)
                    {
                        elite.Momentum[i] = Optimisers[index].Solution[i] - elite.Genes[i];
                        elite.Genes[i] = Optimisers[index].Solution[i];
                    }
                    elite.Fitness = Optimisers[index].Value;
                    Improved[index] = true;
                }
                else
                {
                    elite.Fitness = fitness;
                    Improved[index] = false;
                }

                Handles[index].Reset();

                //Finish
                Work[index] = false;

                Handles[index].WaitOne();
            }
#endif
        }

        public override void Kill()
        {
#if true // In Unity WebGL, multiple threads cannot be used by default.
            throw new System.NotSupportedException("In Unity WebGL, multiple threads cannot be used by default.");
#else
            if (Killed)
            {
                return;
            }

            Killed = true;

            if (UseThreading == true)
            {
                //Stop Threads
                ThreadsRunning = false;
                for (int i = 0; i < Elites; i++)
                {
                    if (Threads[i].IsAlive)
                    {
                        Handles[i].Set();
                        Threads[i].Join();
                    }
                }
            }
#endif
        }

        protected override void SurviveSequential(int index, double timeout)
        {
            //Copy elitist survivor
            Individual survivor = Population[index];
            Individual elite = Offspring[index];
            for (int i = 0; i < Dimensionality; i++)
            {
                elite.Genes[i] = survivor.Genes[i];
                elite.Momentum[i] = survivor.Momentum[i];
            }

            //Exploit
            double fitness = Models[index].ComputeLoss(elite.Genes);
            Optimisers[index].Minimise(elite.Genes, timeout);
            if (Optimisers[index].Value < fitness)
            {
                for (int i = 0; i < Dimensionality; i++)
                {
                    elite.Momentum[i] = Optimisers[index].Solution[i] - elite.Genes[i];
                    elite.Genes[i] = Optimisers[index].Solution[i];
                }
                elite.Fitness = Optimisers[index].Value;
                Improved[index] = true;
            }
            else
            {
                elite.Fitness = fitness;
                Improved[index] = false;
            }
        }

        public override double[] Optimise(int generations, double[] seed)
        {
#if false // In Unity WebGL, multiple threads cannot be used by default.
            if (UseThreading == true)
            {
                for (int i = 0; i < Elites; i++)
                {
                    if (Threads[i].IsAlive == false)
                    {
                        Threads[i].Start();
                    }
                }
            }
#endif

            _model.Refresh();

            for (int i = 0; i < Dimensionality; i++)
            {
                LowerBounds[i] = _model.MotionPtrs[i].Motion.GetLowerLimit(true);
                UpperBounds[i] = _model.MotionPtrs[i].Motion.GetUpperLimit(true);
                Constrained[i] = _model.MotionPtrs[i].Motion.Constrained;
                Solution[i] = seed[i];
            }
            Fitness = _model.ComputeLoss(_solutions);

            if (_model.CheckConvergence(_solutions) == false)
            {
                Initialise(seed);
                for (int i = 0; i < Elites; i++)
                {
                    Models[i].CopyFrom(_model);
                    Optimisers[i].LowerBounds = LowerBounds;
                    Optimisers[i].UpperBounds = UpperBounds;
                }
                for (int i = 0; i < generations; i++)
                {
                    //for(int i=0; i<25; i++) { //Performance testing
                    Evolve();
                }
            }

            return _solutions;
        }
    }

}
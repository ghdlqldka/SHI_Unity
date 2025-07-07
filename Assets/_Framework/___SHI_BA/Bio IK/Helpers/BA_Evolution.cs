using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using BioIK;
using System;

namespace _SHI_BA
{

	//----------------------------------------------------------------------------------------------------
	//====================================================================================================
	//Memetic Evolutionary Optimisation
	//====================================================================================================
	//----------------------------------------------------------------------------------------------------
	public class BA_Evolution : BioIK._Evolution
    {
        private static string LOG_FORMAT = "<color=#50BA4B><b>[BA_Evolution]</b></color> {0}";

        protected new BA_Model _model
        {
            get
            {
                return Model as BA_Model;
            }
        }

        public BA_Evolution(BA_Model model, int populationSize, int elites) : base()
        {
            // Debug.LogError("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@=> ");
            // Debug.Log("Evolution.constructor, useThreading : " + useThreading);
            Model = model;
			PopulationSize = populationSize;
			Elites = elites;
			Dimensionality = model.GetDoF();
            UseThreading = false;

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
                Models[index] = new BA_Model(_model.GetCharacter() as BA_BioIK);
                Optimisers[index] = new BFGS(Dimensionality, x => Models[index].ComputeLoss(x), y => Models[index].ComputeGradient(y, 1e-5));
            }

            ThreadsRunning = false;
		}

        protected override void SurviveSequential(int index, double evolveDuration)
        { // evolveDuration으로 변수명 변경 (명확성 위해)
            //Copy elitist survivor
            Individual survivor = Population[index];
            Individual elite = Offspring[index];
            for (int i = 0; i < Dimensionality; i++)
            {
                elite.Genes[i] = survivor.Genes[i];
                elite.Momentum[i] = survivor.Momentum[i];
            }

            // Exploit (지역 최적화 실행)
            double fitness = Models[index].ComputeLoss(elite.Genes);

            // ======================== [핵심 수정: BFGS 타임아웃 강제 제한] ========================
            // BFGS가 한 번의 호출에서 소모할 최대 시간을 매우 짧게 제한합니다.
            // Time.deltaTime은 현재 프레임에 걸린 시간이므로, 이것의 일부만 사용하도록 합니다.
            // 예를 들어, 0.01 (10ms)은 대략 60FPS의 60%에 해당합니다. 0.005 (5ms)는 30%입니다.
            // WebGL에서 멈춤 현상을 방지하려면 이 값을 매우 짧게 설정하는 것이 중요합니다.
            double bfgsMaxTime = 0.005; // 5ms (조정 가능)
            // 또는, 만약 더 유연하게 제어하고 싶다면, BioIK 컴포넌트에 이 값을 추가하여 Inspector에서 조절 가능하게 할 수 있습니다.
            // Optimisers[index].Minimise(elite.Genes, bfgsMaxTime); // 직접 값 하드코딩
            // =======================================================================================
            // 현재는 evolveDuration을 넘겨주고 있으니, 이것을 짧은 고정값으로 변경합니다.
            Optimisers[index].Minimise(elite.Genes, bfgsMaxTime);


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
            _model.Refresh();

            for (int i = 0; i < Dimensionality; i++)
            {
                LowerBounds[i] = _model.MotionPtrs[i].Motion.GetLowerLimit(true);
                UpperBounds[i] = _model.MotionPtrs[i].Motion.GetUpperLimit(true);
                Constrained[i] = _model.MotionPtrs[i].Motion.Constrained;
                try
                {
                    Solution[i] = seed[i];
                }
                catch (IndexOutOfRangeException ex)
                {
#if DEBUG
                    Debug.LogErrorFormat(LOG_FORMAT, "Dimensionality : " + Dimensionality + ", Solution.Length : " + Solution.Length + ", seed.Length : " + seed.Length);
                    Debug.LogErrorFormat(LOG_FORMAT, "" + ex.Message);
#endif
                }
                
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
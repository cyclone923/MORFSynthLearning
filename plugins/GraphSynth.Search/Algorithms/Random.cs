﻿using GraphSynth.Representation;
using System;
using System.IO;
using OpenBabelFunctions;
using GraphSynth.Search.Tools;



namespace GraphSynth.Search.Algorithms {

    /// <summary>
    /// Randomly select a valid option at any step.
    /// </summary>
    public class Random : AbstractAlgorithm
    {

        public Random(GlobalSettings settings) : base(settings)
        {
            
        }


        public candidate ChooseAndApplyOption(candidate cand)
        {
            var options = GetAvailableOptions(cand);
            return options.Count > 0 ? CopyAndApplyOption(options[Rand.Next(options.Count)], cand, true) : null;
        }

        public candidate ChooseAndApplyCarboxOption(candidate cand)
        {
            var options = GetCarboxylOptions(cand);
            return options.Count > 0 ? CopyAndApplyOption(options[Rand.Next(options.Count)], cand, true) : null;
        }

        public candidate ChooseAndApplyCarboxOptionBestAngle(candidate cand)
        {
            var options = GetCarboxylOptions(cand);
            option bestOpt = null;
            var bestAngle = .0;
            foreach (var opt in options) 
            {
                var evalcand = CopyAndApplyOption(opt, cand, true);
                var mol = OBFunctions.designgraphtomol(evalcand.graph);
                var angle = CalAngle(mol);
                if (angle > 180)
                {
                    Console.WriteLine(angle + " too large");
                    Environment.Exit(0);
                }
                if (angle > bestAngle)
                {
                    bestAngle = angle;
                    bestOpt = opt;
                }
            }
            return bestOpt == null ? null : CopyAndApplyOption(bestOpt, cand, true);
        }

        public candidate ChooseAndApplyCarboxOptionUsingEstimator(candidate cand, Computation cpt, MessageClient clt, string runDir, int epoch)
        {
            option bestOpt = null;
            var bestProperty = Double.NegativeInfinity;
            var options = GetCarboxylOptions(cand);
            foreach (var opt in options) 
            {
                var evalcand = CopyAndApplyOption(opt, cand, true);
                var mol = OBFunctions.designgraphtomol(evalcand.graph);
                var linkerName = AbstractAlgorithm.GetLinkerName(evalcand) + "-E" + epoch.ToString();
                var coeff = Path.Combine(runDir, "data", "linker" + linkerName + ".coeff");
                var lmpdat = Path.Combine(runDir, "data", "linker" + linkerName + ".lmpdat");
                Converter.moltoUFF(OBFunctions.designgraphtomol(evalcand.graph), coeff, lmpdat, false, 100);
                cpt.CalculateFeature(linkerName);
                var properpty = Convert.ToDouble(clt.SendMessage("[Predict]" + " " + linkerName));

                if (properpty > bestProperty)
                {
                    bestProperty = properpty;
                    bestOpt = opt;
                }
            }
            Console.WriteLine("Best {0}", bestProperty);
            return bestOpt == null ? null : CopyAndApplyOption(bestOpt, cand, true);
        }


    }



}
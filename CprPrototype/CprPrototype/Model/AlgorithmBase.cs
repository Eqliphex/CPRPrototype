﻿using AdvancedTimer.Forms.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace CprPrototype.Model
{
    //================================================================================
    //================================================================================
    /// <summary>
    /// The AlgorithmBase class represents a collection of algorithm steps,
    /// resulting in the digital form of CPR algorithm.
    /// </summary>
    public class AlgorithmBase : IDisposable, INotifyPropertyChanged
    {
        #region Properties

        //private List<AlgorithmStep> stepsCollection;
        private List<Drug> drugsCollection;
        private DateTime? startTime;
        private TimeSpan stepTime;
        private AlgorithmStep firstStep, shockable1, smallShock2, nonShockable1, nonShockable2, exitStep, currentStep;

        private int totalElapsedCycles = 0;


        private const int FIRST_CYCLE = 1;

        /// <summary>
        /// Property Changed event handler.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Current step in the algorithm.
        /// </summary>
        public AlgorithmStep CurrentStep
        {
            get { return currentStep; }
            set
            {
                if (currentStep != value)
                {
                    currentStep = value;

                    if (PropertyChanged != null)
                    {
                        OnPropertyChanged("CurrentStep");
                    }
                }
            }
        }

        /// <summary>
        /// Returns the remaining step time.
        /// </summary>
        public TimeSpan StepTime
        {
            get { return stepTime; }
            set
            {
                if (stepTime != value)
                {
                    stepTime = value;

                    if (PropertyChanged != null)
                    {
                        OnPropertyChanged("StepTime");
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of cycles the algorithm went through.
        /// </summary>
        public int TotalElapsedCycles { get { return totalElapsedCycles; } }

        /// <summary>
        /// Returns the first step in the algorithm.
        /// </summary>
        public AlgorithmStep FirstStep { get { return firstStep; } }

        /// <summary>
        /// Returns the starting Date and Time for current
        /// resuscitation process.
        /// </summary>
        public DateTime? StartTime { get { return startTime; } }

        #endregion

        #region Construction & Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public AlgorithmBase()
        {
            InitializeAlgorithmSteps();
            drugsCollection = new DrugFactory().CreateDrugs();
            StepTime = TimeSpan.FromMinutes(2);

            // Initiate so we start at firstStep
            CurrentStep = firstStep;
        }

        /// <summary>
        /// Construct and Initialize algorithm steps.
        /// </summary>
        public void InitializeAlgorithmSteps()
        {            
            //========================================================================
            // Initial Step
            //========================================================================

            firstStep = new AlgorithmStep("Vurder Rytmen", "Vurder patientens rytme")
            {
                StepType = StepType.AssessmentStep
            };

            //========================================================================
            // Shockable Steps
            //========================================================================

            shockable1 = new AlgorithmStep("Stød en gang", "Fortsæt HLR")
            {
                RythmStyle = RythmStyle.Shockable,
                StepType = StepType.HLRStep
            };

            //========================================================================
            // Non-Shockable Steps
            //========================================================================

            nonShockable1 = new AlgorithmStep("Giv 1mg Adrenalin", "Fortsæt HLR")
            {
                RythmStyle = RythmStyle.NonShockable,
                StepType = StepType.HLRStep
            };

            nonShockable2 = new AlgorithmStep("Fortsæt HLR ", "Fortsæt HLR")
            {
                RythmStyle = RythmStyle.NonShockable,
                StepType = StepType.HLRStep
            };

            //========================================================================
            // Exit Step (currently not used)
            //========================================================================

            exitStep = new AlgorithmStep("Circulation restored", "Continue with further resuscitation")
            {
                StepType = StepType.AssessmentStep
            };

            //========================================================================
            // Setup Step Relations
            //========================================================================

            firstStep.PreviousStep = null;

            shockable1.PreviousStep = firstStep;
            shockable1.NextStep = firstStep;

            nonShockable1.PreviousStep = firstStep;
            nonShockable1.NextStep = firstStep;

            nonShockable2.PreviousStep = firstStep;
            nonShockable2.NextStep = firstStep;

            exitStep.PreviousStep = firstStep;
        }

        #endregion

        #region Timer Related Methods

        /// <summary>
        /// Adds drug shots to the drug queue if it
        /// does not exist in it already.
        /// </summary>
        public void AddDrugsToQueue(ObservableCollection<DrugShot> list, RythmStyle rythmStyle)
        {
            if (drugsCollection != null && drugsCollection.Count > 0)
            {
                foreach (Drug drug in drugsCollection)
                {
                    var shot = drug.GetDrugShot(TotalElapsedCycles, rythmStyle);

                    if (shot != null && !list.Contains(shot))
                    {
                        if (TotalElapsedCycles == 0 && CurrentStep.NextStep.RythmStyle == RythmStyle.NonShockable)
                        {
                            shot.TimeRemaining = TimeSpan.FromMinutes(2);
                            list.Add(shot);
                        }
                        else
                        {
                            shot.ResetShot();
                            list.Add(shot);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes Expired Drug Reminders form queue.
        /// </summary>
        public void RemoveDrugsFromQueue(ObservableCollection<DrugShot> list)
        {
            if (drugsCollection != null && drugsCollection.Count > 0)
            {
                foreach (DrugShot shot in list)
                {
                    var drug = drugsCollection.Find(x => x.DrugType == shot.Drug.DrugType);

                    if (shot.IsInjected)
                    {
                        list.Remove(shot);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Initiates the CPR sequence based on
        /// the provided rythm style.
        /// </summary>
        public void BeginSequence(RythmStyle rythmStyle)
        {
            if (startTime == null)
            {
                startTime = DateTime.Now;
            }

            switch (rythmStyle)
            {
                case RythmStyle.Shockable:
                    CurrentStep.NextStep = shockable1;
                    break;
                case RythmStyle.NonShockable: 
                    CurrentStep.NextStep = nonShockable2;
                    break;
            }
        }

        /// <summary>
        /// Advances to the next step.
        /// </summary>
        public void AdvanceOneStep()
        {
            var next = CurrentStep.NextStep;

            // Update Cycle count
            if (next.StepType.Equals(StepType.AssessmentStep))
            {
                totalElapsedCycles++;
            }

            // Sanity check
            if (next != null)
            {
                CurrentStep = next;
            }
            else
            {
                throw new OperationCanceledException("AlgorithmBase : AdvanceOneStep() - Next step is null.");
            }
        }

        /// <summary>
        /// Event handler for INotifyPropertyChanged.
        /// </summary>
        /// <param name="propertyName">Name of the property changed. Optional</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
                Debug.WriteLine("AlgorithmPropertyChanged - " + propertyName);
            }
        }

        /// <summary>
        /// IDisposible implementation.
        /// </summary>
        public void Dispose()
        {
            //this.stepsCollection = null;
        }
    }
}
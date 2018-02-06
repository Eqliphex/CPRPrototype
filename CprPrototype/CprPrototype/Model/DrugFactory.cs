﻿using System;
using System.Collections.Generic;

namespace CprPrototype.Model
{
    /// <summary>
    /// The DrugFactory class is responsible for the construction
    /// of all drugs and their dosages.
    /// </summary>
    public class DrugFactory
    {
        /// <summary>
        /// Creates a list of Drugs, including dosage for each drug.
        /// </summary>
        /// <param name="prepTimeMinutes">Optional - Drug Prep Time in Minutes. (Default = 3.0)</param>
        /// <returns>List of Drugs</returns>
        public List<Drug> CreateDrugs(double prepTimeMinutes = 3)
        {
            var result = new List<Drug>();
            var prepTime = TimeSpan.FromMinutes(prepTimeMinutes);

            // Adrenalin
            var adrenalinDrug = new Drug(DrugType.Adrenalin, prepTime);
            adrenalinDrug.DosesCollection.Add(new DrugShot(adrenalinDrug, "1mg"));
            result.Add(adrenalinDrug);

            // Amiodoran
            var amiodoranDrug = new Drug(DrugType.Amiodaron, prepTime);
            amiodoranDrug.DosesCollection.Add(new DrugShot(amiodoranDrug, "300ml"));
            amiodoranDrug.DosesCollection.Add(new DrugShot(amiodoranDrug, "150ml"));
            result.Add(amiodoranDrug);

            // TODO: Add extra drugs

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTest
{
    public class RecipeInfoMessage
    {
        //public Int16 State { get; set; }

        public RecipeOverride GraphSpec { get; set; }

        public float ConversionRate { get; set; }

        public float ThicknessMax { get; set; }

        public float ThicknessMin { get; set; }

        public float Ior { get; set; }

        public float StartWave { get; set; }

        public float EndWave { get; set; }

        public string LayerName { get; set; }

        public string UnitOfMeasure { get; set; }

        public RecipeInfoMessage()
        {
            GraphSpec = new RecipeOverride();
        }

        public RecipeInfoMessage(RecipeInfo recipe)
        {
            GraphSpec = new RecipeOverride();
            GraphSpec = recipe.GraphSpec;
            ConversionRate = recipe.ConversionRate;
            ThicknessMax = recipe.ThicknessMax;
            ThicknessMin = recipe.ThicknessMin;
            Ior = recipe.Ior;
            StartWave = recipe.StartWave;
            EndWave = recipe.EndWave;
            LayerName = Encoding.ASCII.GetString(recipe.LayerName, 2, recipe.LayerName[1]);
            UnitOfMeasure = Encoding.ASCII.GetString(recipe.UnitOfMeasure, 2, recipe.UnitOfMeasure[1]);
        }
    }
}

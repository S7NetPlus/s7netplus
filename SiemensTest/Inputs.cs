using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTest
{
    public class Inputs
    {
        public RuntimeOperations RuntimeInputs { get; set; }

        public RecipeOverride SpecOverride { get; set; }

        public WebDimensions LaneOverride { get; set; }

        public Inputs()
        {
            RuntimeInputs = new RuntimeOperations();
            SpecOverride = new RecipeOverride();
            LaneOverride = new WebDimensions();
        }
    }
}

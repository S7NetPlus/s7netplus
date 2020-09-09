using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTest
{
    public class RecipeInfo
    {
        private byte[] m_layerName = new byte[254];
        private byte[] m_unitOfMeasure = new byte[254];

        //public Int16 State { get; set; }

        public RecipeOverride GraphSpec { get; set; }

        public float ConversionRate { get; set; }

        public float ThicknessMax { get; set; }

        public float ThicknessMin { get; set; }

        public float Ior { get; set; }

        public float StartWave { get; set; }

        public float EndWave { get; set; }

        public byte[] LayerName
        {
            get { return m_layerName; }

            set { m_layerName = value; }
        }

        public byte[] UnitOfMeasure
        {
            get { return m_unitOfMeasure; }

            set { m_unitOfMeasure = value; }
        }

        public RecipeInfo()
        {
            GraphSpec = new RecipeOverride();
        }

        public RecipeInfo(RecipeInfoMessage recipe)
        {
            GraphSpec = new RecipeOverride();
            GraphSpec = recipe.GraphSpec;
            ConversionRate = recipe.ConversionRate;
            ThicknessMax = recipe.ThicknessMax;
            ThicknessMin = recipe.ThicknessMin;
            Ior = recipe.Ior;
            StartWave = recipe.StartWave;
            EndWave = recipe.EndWave;
            m_layerName = S7.Net.Types.StringEx.ToByteArray(recipe.LayerName, 254);
            Array.Resize(ref m_layerName, 256);
            m_unitOfMeasure = S7.Net.Types.StringEx.ToByteArray(recipe.UnitOfMeasure, 254);
            Array.Resize(ref m_unitOfMeasure, 256);
        }
    }
}

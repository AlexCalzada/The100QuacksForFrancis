using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel
{
    public class TXDiestel
    {
        private  ArrayList alRequest = new ArrayList();
        private  ArrayList alResponse = new ArrayList();
        private string SKU;

        public void Init()
        {
            SKU = string.Empty;
            alRequest.Clear();
            alResponse.Clear();
        }

        public void setSKU(string sSKU)
        {
            SKU = sSKU;
        }

        public void AddToRequest(DiestelField NewField)
        {
            alRequest.Add(NewField);
        }

        public void AddToResponse(DiestelField NewField)
        {
            alResponse.Add(NewField);
        }
    }

    public class DiestelField
    {
        public string DataType { get; set; }

        public int DataLength { get; set; }

        public string Name { get; set; }

        public DiestelField() { }

        public DiestelField(string DataType, int DataLength, string ProdName)
        {
            this.DataType = DataType;
            this.DataLength = DataLength;
            this.Name = ProdName;
        }
    }
}

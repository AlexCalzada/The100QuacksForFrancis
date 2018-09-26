using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.ExtensionMethods
{
    public static class FormaPago
    {
        public static string AsText(this TipoPago pago)
        {
            switch (pago)
            {
                case TipoPago.Efectivo:
                    return "EFE";
                case TipoPago.TarjetaCredito:
                    return "TC";
                case TipoPago.TarjetaDebito:
                    return "TD";
            }

            return "N/A";
        }
    }

    public enum TipoPago
    {
        Efectivo,
        TarjetaCredito,
        TarjetaDebito
    }
}

using FromFrancisToLove.Data;
using FromFrancisToLove.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel.Clases
{
    public class DbCrud
    {
        private readonly HouseOfCards_Context _context;

        public DbCrud(HouseOfCards_Context context)
        {
            _context = context;
        }

        public bool CheckTx(int UserID, long transaction)
        {
            try
            {
                var userTransactionEstatus = _context.transaccionesTest.Where(x => x.UserId == UserID).SingleOrDefault().Estatus;
                if (userTransactionEstatus == 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return true;
                throw;
            }
            return true;
        }

        //-----------

        public (bool estatus, int transactionStatus, long currentTransaction) CheckTransaction(int _User)
        {
            var Status = _context.transaccionesTest.Where(x => x.UserId == _User).SingleOrDefault().Estatus;
            var tx = _context.transaccionesTest.Where(x => x.UserId == _User).SingleOrDefault().TransaccionId;

            if (Status == 0)
            {
                return (true, Status, tx);
            }
            else if (Status != 0)
            {
                return (false, Status, tx);
            }

            return (false, -1, -1);
        }

        public bool InsertInitialTransaction(int _User, string _SKU, string _Reference)
        {
            try
            {
                var transaction = _context.Set<Transacciones>();
                transaction.Add(
                                 new Transacciones
                                 {
                                     WSProveedorId = 1,
                                     UserId = _User,
                                     SKU = _SKU,
                                     Fecha = DateTime.Now,
                                     Referencia = _Reference,
                                     Monto = 0.00M,
                                     Comision_FF = 5.00M,
                                     Comision_WS = 0.00M,
                                     Pago = "N/A",
                                     sConsulta = "N/A",
                                     sCancelacion = "N/A",
                                     Status_Interno = 0,
                                     Status_Inicial = 0,
                                     Status_CodeResponse = 0,
                                     Estatus = 0
                                 }
                               );
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateUserTransaction(long currentTransaction)
        {

            try
            {
                var transaction = _context.transaccionesTest.Where(x => x.TransaccionId == currentTransaction).FirstOrDefault();

                transaction.sConsulta = "Servicio solicitado en proceso";
                transaction.Status_Inicial = 1;
                transaction.Status_Interno++;

                _context.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateTxTest(string serviceQuery, long userTransaction)
        {
            try
            {
                var tx = _context.transaccionesTest.Where(x => x.TransaccionId == userTransaction).FirstOrDefault();

                if (tx.Estatus == 0)
                {
                    tx.sConsulta = serviceQuery;
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

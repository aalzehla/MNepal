using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.IRepository;

namespace MNepalProject.Repository
{
    public class MNRequestRepository:IMNRequestRepository,IDisposable
    {
        private PetaPoco.Database database;
        private MNRequest mnRequest;
        private string DBName = "MNRequest";
        private bool disposed = false;

        public MNRequestRepository(PetaPoco.Database database)
        {
            this.database = database;
        }


        public string InsertIntoMNRequestToCreateNewUser(MNRequest mnrequest)
        {

            string reply = "";
            if (mnrequest != null)
            {
                /*
                    this.OriginID = OriginID;
                    this.ServiceCode = ServiceCode;
                    this.SourceBankCode = SourceBankCode;
                    this.SourceBranchCode = SourceBranchCode;
                    this.SourceAccountNo = SourceAccountNo;
                    this.Amount = Amount;
                    this.FeeId = FeeId;
                    this.TraceNo = TraceNo;
                    this.TranDate = TranDate;
                    this.RetrievalRef = RetrievalRef;
                    this.Desc1 = Desc1;
                    this.IsProcessed = IsProcessed;
                         */
                database.Execute("Insert into MNRequest(OriginID,OriginType,ServiceCode,SourceBankCode,SourceBranchCode,SourceAccountNo,Amount,FeeId,TraceNo,TranDate,RetrievalRef,Desc1,IsProcessed)" +
                                                        " values(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12)", 
                    mnrequest.OriginID,
                    mnrequest.OriginType,
                    mnrequest.ServiceCode,
                    mnrequest.SourceBankCode,
                    mnrequest.SourceBranchCode,
                    mnrequest.SourceAccountNo,
                    mnrequest.Amount,
                    mnrequest.FeeId,
                    mnrequest.TraceNo,
                    mnrequest.TranDate,
                    mnrequest.RetrievalRef,
                    mnrequest.Desc1,
                    mnrequest.IsProcessed);

                    reply = "true";
            }
            else
            {
                reply = "false";

            }
            return reply;
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    database.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
    }
}
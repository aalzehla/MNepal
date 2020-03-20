using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.Controllers;
using MNepalProject.DAL;

namespace MNepalProject.Services
{
    public class AgentDetails
    {
        public MNAgent getAgentDetails { get; private set; }
        public MNClient getDetails { get; set; }
        //IEnumerable<MNBankAccountMap>
     

        public MNAgent AgentDetailsByAgentDetailsFromMNClient(MNAgent mnagent)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            try
            {
                if (mnagent.ClientCode != null)
                {
                    getAgentDetails = dataContext.Single<MNAgent>("select * from MNAgent where ClientCode=@0", mnagent.ClientCode);
                    if (getAgentDetails!=null)
                    {
                        return getAgentDetails;
                    }
                    else
                    {
                        return getAgentDetails;
                    }
                   
                }
                else
                {
                    return getAgentDetails;
                }

            }
            catch (Exception ex)
            {
                return getAgentDetails;
            }

            return getAgentDetails;

        }

        public MNClient AgentDetailsByAgentDetailsFromAgentId(MNAgent mnagent)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            try
            {
                if (mnagent.ID != 0)
                {
                    getDetails = dataContext.Single<MNClient>("select * from MNClient where ClientCode in (select ClientCode from MNAgent where ID=@0)", mnagent.ID);
                    if (getDetails != null)
                    {
                        return getDetails;
                    }
                    else
                    {
                        return getDetails;
                    }

                }
                else
                {
                    return getDetails;
                }

            }
            catch (Exception ex)
            {
                return getDetails;
            }

        }
        
    }
}
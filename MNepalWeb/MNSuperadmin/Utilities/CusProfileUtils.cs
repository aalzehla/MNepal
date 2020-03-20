using MNSuperadmin.Models;
using MNSuperadmin.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace MNSuperadmin.Utilities
{
    public class CusProfileUtils
    {

        public string GetChargesXMLStr(List<MNProfileChargeClass> ProfileChgList)
        {
            /*OUTPUT
            <Charges>
              <Charge>
                <ProfileCode>TEST</ProfileCode>
                <Registration>200</Registration>
                <ReNew>100</ReNew>
                <PinReset>100</PinReset>
                <ChargeAccount>001123456879</ChargeAccount>
              </Charge>
            </Charges>
            */

            string retStr = string.Empty;

            var xmlfromLINQ = new XElement("Charges",
            from chg in ProfileChgList
            select new XElement("Charge",
                new XElement("ProfileCode", chg.ProfileCode),
                new XElement("Registration", chg.Registration),
                new XElement("ReNew", chg.ReNew),
                new XElement("PinReset", chg.PinReset),
                new XElement("ChargeAccount", chg.ChargeAccount)
                ));

            retStr = xmlfromLINQ.ToString();
            return retStr;

        }

        public string GetServiceAccountXMLStr(List<TransactionInfo> ServiceAccountList)
        {
            /*OUTPUT
            <ServiceAccounts>
              <ServiceAccount>
                <AcNumber>0010101010</AcNumber>
                <Alias>Main Account</Alias>
                <AcOwner>Self</AcOwner>
                <IsPrimary>T</IsPrimary>
                <AcType>10</AcType>
                <TxnEnabled>F</TxnEnabled>
                <TBranchCode>001</TBranchCode>
              </ServiceAccount>
            </ServiceAccounts>
            */


            if (ServiceAccountList.Count <= 0)
            {
                return null;
            }
            string retStr = string.Empty;
            var xmlfromLINQ = new XElement("ServiceAccounts",
                from txn in ServiceAccountList
                select new XElement("ServiceAccount",
                new XElement("AcNumber", txn.AcNumber),
                new XElement("Alias", txn.Alias),
                new XElement("AcOwner", txn.AcOwner),
                new XElement("IsPrimary", txn.IsPrimary),
                new XElement("AcType", txn.AcType),
                new XElement("TxnEnabled", txn.TxnEnabled),
                new XElement("TBranchCode", txn.TBranchCode)
                ));

            retStr = xmlfromLINQ.ToString();
            return retStr;

        }



        public string GetTxnLimitXMLStr(List<MNTxnLimitClass> TxnLimitList)
        {
            /*OUTPUT
            <TxnLimits>
              <TxnLimit>
                <ProfileCode>TEST</ProfileCode>
                <FeatureCode>01</FeatureCode>
                <TxnCount>10</TxnCount>
                <PerTxnAmt>5000</PerTxnAmt>
                <PerDayAmt>10000</PerDayAmt>
                <TxnAmtM>10</TxnAmtM>
              </TxnLimit>
            </TxnLimits>
            */


            if (TxnLimitList.Count <= 0)
            {
                return null;
            }
            string retStr = string.Empty;
            var xmlfromLINQ = new XElement("TxnLimits",
                from txn in TxnLimitList
                select new XElement("TxnLimit",
                new XElement("ProfileCode", txn.ProfileCode),
                new XElement("FeatureCode", txn.FeatureCode),
                new XElement("TxnCount", txn.TxnCount),
                new XElement("PerTxnAmt", txn.PerTxnAmt),
                new XElement("PerDayAmt", txn.PerDayAmt),
                new XElement("TxnAmtM", txn.TxnAmtM)
                ));

            retStr = xmlfromLINQ.ToString();
            return retStr;

        }


        public string GetMakerCheckerXMLStr(List<MNMakerChecker> MakerCheckerList)
        {
            if (MakerCheckerList.Count <= 0)
            {
                return null;
            }
            string retStr = string.Empty;
            var xmlfromLINQ = new XElement("MakerCheckers",
                from item in MakerCheckerList
                select new XElement("MakerChecker",
               // new XElement("Code", item.Code),
                new XElement("TableName", item.TableName),
                new XElement("ColumnName", item.ColumnName),
                new XElement("Module", item.Module),
                new XElement("OldValue", item.OldValue),
                new XElement("NewValue", item.NewValue)
             
               // new XElement("BranchCode", item.BranchCode),
                //new XElement("EditedBy", item.EditedBy),
                //new XElement("EditedOn", item.EditedOn)
                ));

            retStr = xmlfromLINQ.ToString();
            return retStr;
        }





        //public string GetContactXMLStr(List<MNContactClass> ContactList)
        //{
        //    /*OUTPUT
        //    <Contacts>
        //      <Contact>
        //        <ClientCode>00112345</ClientCode>
        //        <Address>Kathmandu</Address>
        //        <Contact1>9841123456</Contact1>
        //        <Contact2></Contact2>
        //        <Email>email@gmail.com</Email>
        //      </Contact>
        //    </Contacts>
        //    */

        //    string retStr = string.Empty;

        //    var xmlfromLINQ = new XElement("Contacts",
        //        from contact in ContactList
        //        select new XElement("Contact",
        //        new XElement("ClientCode", contact.ClientCode),
        //        new XElement("Address", contact.Address),
        //        new XElement("Contact1", contact.ContactNum1),
        //        new XElement("Contact2", contact.ContactNum2),
        //        new XElement("Email", contact.EmailAddress)
        //        ));

        //    retStr = xmlfromLINQ.ToString();
        //    return retStr;

        //}

    }

}
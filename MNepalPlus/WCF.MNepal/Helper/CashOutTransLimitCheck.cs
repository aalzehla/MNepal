﻿using MNepalProject.Models;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Globalization;
using System.Net;
using System.ServiceModel.Web;
using WCF.MNepal.Models;
using WCF.MNepal.Utilities;

namespace WCF.MNepal.Helper
{
    public class CashOutTransLimitCheck
    {
        public string CashOutLimitCheck(string mobile, string destmobile, string amount, string sc, string pin, string src)
        {
            string hasKYC = string.Empty;
            string HasBankKYC = string.Empty;
            int transCount = 0;
            decimal singleTransLimit = 0;
            decimal DailyLimit = 0;
            decimal MonthlyLimit = 0;

            string EDIndvTxn = string.Empty;
            string DateRange = string.Empty;
            string StartDate = string.Empty;
            string EndDate = string.Empty;
            decimal IndvTransLimit = 0;
            string totalIndvDailyAmount = string.Empty;
            string totalIndvMonthlyAmount = string.Empty;
            decimal totIndvTxnDailyAmt = 0;
            decimal totIndvTxnMonthlyAmt = 0;
            decimal totIndvDailyAmt = 0;
            decimal totIndvMonthlyAmt = 0;
            string LimitType = string.Empty;
            int IndvTransCount = 0;
            decimal IndvTransLimitMonthly = 0;
            decimal IndvTransLimitDaily = 0;

            string totalAmount = string.Empty;
            string totalCount = "0";
            string balance = string.Empty;
            string BankBaln = string.Empty;
            string totalMonthlyTran = string.Empty;

            string statusCode = string.Empty;
            string message = string.Empty;
            string customerNo = string.Empty;

            decimal totTxnAmt = 0;
            decimal totAmt = 0;
            decimal baln = 0;
            decimal amtInt32 = 0;
            decimal totMonthlyTxnAmt = 0;
            decimal totMonthlyAmt = 0;
            string result = string.Empty;
            string errorpin = string.Empty;

            amtInt32 = decimal.Parse(amount);

            ReplyMessage replyMessage = new ReplyMessage();

            CustActivityModel custsmsInfo = new CustActivityModel();

            DataTable dtableUserCheck = CustCheckUtils.GetCustUserInfo(mobile);
            if (dtableUserCheck.Rows.Count == 0)
            {
                customerNo = "0";
            }
            else if (dtableUserCheck.Rows.Count > 0)
            {
                customerNo = mobile;
            }

            if (customerNo != "0")
            {
               
               
                 DataTable dtableResult = PinUtils.GetPinInfo(destmobile);
                if (dtableResult != null)
                {
                    foreach (DataRow dtableUser in dtableResult.Rows)
                    {
                        String validPIN = dtableUser["PIN"].ToString();
                        if (validPIN.Equals(pin))
                        {
                            pin = validPIN;
                        }
                        else
                        {
                            errorpin = pin;
                            pin = "";
                        }
                    }
                }


                if (pin != "")
                {
                    String validStatus = "";
                    DataTable dtableStatusResult = CustCheckUtils.GetCustStatusInfo(mobile);
                    if (dtableStatusResult != null)
                    {
                        foreach (DataRow dtableUser in dtableStatusResult.Rows)
                        {
                            validStatus = dtableUser["Status"].ToString();
                        }
                    }

                    if (validStatus == "Expired")
                    {
                        statusCode = "400";
                        message = "Account is Blocked";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                    }
                    if (validStatus == "Blocked")
                    {
                        statusCode = "400";
                        message = "Account is Blocked";
                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                    }
                    if (validStatus == "Active")
                    {
                        //Check KYC of customer
                        ////////////////////////////////////
                        ///////////////////////////////////

                        DataTable dtCheckKYC = WalletLimitUtils.CheckCustKYC(mobile);
                        if (dtCheckKYC.Rows.Count > 0)
                        {
                            hasKYC = Convert.ToString(dtCheckKYC.Rows[0]["HasKYC"].ToString() == "" ? "0" : dtCheckKYC.Rows[0]["HasKYC"].ToString());
                        }
                        DataTable dtCheckBankKYC = WalletLimitUtils.CheckCustBankKYC(mobile);
                        if (dtCheckBankKYC.Rows.Count > 0)
                        {
                            HasBankKYC = Convert.ToString(dtCheckBankKYC.Rows[0]["HasBankKYC"].ToString() == "" ? "0" : dtCheckBankKYC.Rows[0]["HasBankKYC"].ToString());
                        }
                        //Without KYC N WithOut Bank Link
                        if ((hasKYC == "F") || (hasKYC == "P") && (HasBankKYC == "F") || (HasBankKYC == "P"))
                        {
                            /*WithOut KYC W2W Only */
                            /// 00 - Wallet To Wallet
                            /// 30 - W2B Merchant Payment
                            /// 31 - Wallet W2B Utility Payment
                            /// 32 - W2B Coupon Recharge
                            if ((sc == "00") || (sc == "30") || (sc == "31") || (sc == "32"))
                            {
                                //Transaction Limit Wallet
                                DataTable dtTransWalletLimitNonKYC = WalletLimitUtils.GetWalletLimitNonKYC(mobile, sc);
                                if (dtTransWalletLimitNonKYC.Rows.Count > 0)
                                {
                                    DailyLimit = Convert.ToDecimal(dtTransWalletLimitNonKYC.Rows[0]["WPerDayAmt"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WPerDayAmt"].ToString());
                                    MonthlyLimit = Convert.ToDecimal(dtTransWalletLimitNonKYC.Rows[0]["WTxnAmtM"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WTxnAmtM"].ToString());
                                    singleTransLimit = Convert.ToDecimal(dtTransWalletLimitNonKYC.Rows[0]["WPerTxnAmt"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WPerTxnAmt"].ToString());
                                    transCount = Convert.ToInt32(dtTransWalletLimitNonKYC.Rows[0]["WTxnCount"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WTxnCount"].ToString());
                                }
                                //////////////////////////////
                                DataTable dtableUserWalletLimit = new DataTable();
                                dtableUserWalletLimit = WalletLimitUtils.GetWalletUserLimitInfo(mobile);
                                if (dtableUserWalletLimit != null)
                                {
                                    if (dtableUserWalletLimit.Rows.Count == 0)
                                    {
                                        totalAmount = "0";
                                    }
                                    else if (dtableUserWalletLimit.Rows.Count > 0)
                                    {
                                        totalAmount = dtableUserWalletLimit.Rows[0]["Amount"].ToString();
                                        totalCount = dtableUserWalletLimit.Rows[0]["TotalCount"].ToString();
                                        balance = dtableUserWalletLimit.Rows[0]["Balance"].ToString();
                                        totalMonthlyTran = dtableUserWalletLimit.Rows[0]["TotalMonthlyWTran"].ToString();

                                        if (totalAmount == "")
                                        {
                                            totalAmount = "0";
                                        }
                                        if (totalMonthlyTran == "")
                                        {
                                            totalMonthlyTran = "0";
                                        }
                                    }

                                    totTxnAmt = decimal.Parse(totalAmount);
                                    totMonthlyTxnAmt = decimal.Parse(totalMonthlyTran);

                                    baln = decimal.Parse(balance);
                                }
                                else
                                {
                                    statusCode = "400";
                                    message = "Insufficient Balance";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                            }
                            
                            //For Indv
                            totIndvDailyAmt = (totIndvTxnDailyAmt) + decimal.Parse(amount);
                            totIndvMonthlyAmt = (totIndvTxnMonthlyAmt) + decimal.Parse(amount);

                            //Total FOR Profile LIMIT
                            totAmt = (totTxnAmt) + decimal.Parse(amount);

                            totMonthlyAmt = (totMonthlyTxnAmt) + decimal.Parse(amount);

                            //START CHECK LIMIT WALLET
                            if ((sc == "00") || (sc == "30") || (sc == "31") || (sc == "32"))
                            {
                                ////CHECK LIMIT
                                if (totMonthlyAmt > MonthlyLimit)
                                {
                                    statusCode = "508";
                                    message = "Monthly Transaction amount limit reached. ";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (Convert.ToInt32(totalCount) > transCount)
                                {
                                    if (totAmt > DailyLimit)
                                    {
                                        statusCode = "508";
                                        message = "Daily Transaction count limit reached.";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                }
                                if (totAmt > DailyLimit)
                                {
                                    statusCode = "508";
                                    message = "Daily Transaction amount limit reached. ";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (Decimal.Parse(amount) > singleTransLimit)
                                {
                                    statusCode = "508";
                                    message = "Cannot perform Transaction exceeding " + singleTransLimit.ToString("#,##0.00");
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (baln < amtInt32)
                                {
                                    if (statusCode != "")
                                    {
                                        statusCode = "400";
                                        String msg = message;
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                    else
                                    {
                                        statusCode = "500";
                                        message = "Insufficient Balance";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }

                                }

                                ////END CHECK LIMIT
                            }
                            else if ((sc == "10") || (sc == "11") || (sc == "33") || (sc == "34") || (sc == "35"))
                            {
                                statusCode = "400";
                                message = "Link your bank account to enjoy more benefits. ";
                                replyMessage.Response = message;
                                replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            }
                            else
                            {
                                statusCode = "400";
                                message = "Update your KYC details to get rid of restriction. ";
                                replyMessage.Response = message;
                                replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            }
                            ////END CHECK LIMIT WALLET
                        }
                        //With KYC N WithOut Bank Link
                        else if ((hasKYC == "T") && (HasBankKYC == "F") || (HasBankKYC == "P"))
                        {
                            /*With KYC W2W N W2B*/
                            /// 00 - Wallet To Wallet
                            /// 01 - Wallet to Bank
                            /// 30 - W2B Merchant Payment
                            /// 31 - Wallet W2B Utility Payment
                            /// 32 - W2B Coupon Recharge
                            if ((sc == "00") || (sc == "01") || (sc == "30") || (sc == "31") || (sc == "32") || (sc == "50") || (sc == "51"))
                            {
                                //Transaction Limit Wallet
                                DataTable dtTransWalletLimitNonKYC = WalletLimitUtils.GetWalletLimitWithKYCOutBank(mobile, sc);
                                if (dtTransWalletLimitNonKYC.Rows.Count > 0)
                                {
                                    DailyLimit = Convert.ToDecimal(dtTransWalletLimitNonKYC.Rows[0]["WPerDayAmt"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WPerDayAmt"].ToString());
                                    MonthlyLimit = Convert.ToDecimal(dtTransWalletLimitNonKYC.Rows[0]["WTxnAmtM"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WTxnAmtM"].ToString());
                                    singleTransLimit = Convert.ToDecimal(dtTransWalletLimitNonKYC.Rows[0]["WPerTxnAmt"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WPerTxnAmt"].ToString());
                                    transCount = Convert.ToInt32(dtTransWalletLimitNonKYC.Rows[0]["WTxnCount"].ToString() == "" ? "0" : dtTransWalletLimitNonKYC.Rows[0]["WTxnCount"].ToString());
                                }
                                //////////////////////////////
                                DataTable dtableUserWalletLimit = new DataTable();
                                dtableUserWalletLimit = WalletLimitUtils.GetWalletUserLimitInfo(mobile);
                                if (dtableUserWalletLimit != null)
                                {
                                    if (dtableUserWalletLimit.Rows.Count == 0)
                                    {
                                        totalAmount = "0";
                                    }
                                    else if (dtableUserWalletLimit.Rows.Count > 0)
                                    {
                                        totalAmount = dtableUserWalletLimit.Rows[0]["Amount"].ToString();
                                        totalCount = dtableUserWalletLimit.Rows[0]["TotalCount"].ToString();
                                        balance = dtableUserWalletLimit.Rows[0]["Balance"].ToString();
                                        totalMonthlyTran = dtableUserWalletLimit.Rows[0]["TotalMonthlyWTran"].ToString();

                                        if (totalAmount == "")
                                        {
                                            totalAmount = "0";
                                        }
                                        if (totalMonthlyTran == "")
                                        {
                                            totalMonthlyTran = "0";
                                        }
                                    }

                                    totTxnAmt = decimal.Parse(totalAmount);
                                    totMonthlyTxnAmt = decimal.Parse(totalMonthlyTran);

                                    baln = decimal.Parse(balance);
                                }
                                else
                                {
                                    statusCode = "400";
                                    message = "Insufficient Balance";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                            }
                            
                            //For Indv
                            totIndvDailyAmt = (totIndvTxnDailyAmt) + decimal.Parse(amount);
                            totIndvMonthlyAmt = (totIndvTxnMonthlyAmt) + decimal.Parse(amount);

                            //Total FOR Profile LIMIT
                            totAmt = (totTxnAmt) + decimal.Parse(amount);

                            totMonthlyAmt = (totMonthlyTxnAmt) + decimal.Parse(amount);

                            //START CHECK LIMIT WALLET
                            if ((sc == "00") || (sc == "01") || (sc == "30") || (sc == "31") || (sc == "32") || (sc == "50") || (sc == "51"))
                            {
                                ////CHECK LIMIT
                                if (totMonthlyAmt > MonthlyLimit)
                                {
                                    statusCode = "508";
                                    message = "Monthly Transaction amount limit reached. ";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (Convert.ToInt32(totalCount) > transCount)
                                {
                                    if (totAmt > DailyLimit)
                                    {
                                        statusCode = "508";
                                        message = "Daily Transaction count limit reached.";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                }
                                if (totAmt > DailyLimit)
                                {
                                    statusCode = "508";
                                    message = "Daily Transaction amount limit reached. ";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (Decimal.Parse(amount) > singleTransLimit)
                                {
                                    statusCode = "508";
                                    message = "Cannot perform Transaction exceeding " + singleTransLimit.ToString("#,##0.00");
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (baln < amtInt32)
                                {
                                    if (statusCode != "")
                                    {
                                        statusCode = "400";
                                        String msg = message;
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                    else
                                    {
                                        statusCode = "500";
                                        message = "Insufficient Balance";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }

                                }

                                ////END CHECK LIMIT
                            }
                            else if ((sc == "10") || (sc == "11") || (sc == "33") || (sc == "34") || (sc == "35") || (sc == "50") || (sc == "51"))
                            {
                                statusCode = "400";
                                message = "Link your bank account to enjoy more benefits. ";
                                replyMessage.Response = message;
                                replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            }
                            else
                            {
                                statusCode = "400";
                                message = "Update your KYC details to get rid of restriction. ";
                                replyMessage.Response = message;
                                replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                            }
                            ////END CHECK LIMIT WALLET
                        }
                        //With KYC N With Bank Link
                        else if ((hasKYC == "T") && (HasBankKYC == "T"))
                        {
                            /// 00 - Wallet To Wallet
                            /// 01 - Wallet to Bank
                            /// 30 - W2B Merchant Payment
                            /// 31 - Wallet W2B Utility Payment
                            /// 32 - W2B Coupon Recharge
                            /// 50 - Cash In Wallet
                            /// 51 - Cash Out Wallet
                            /// SC: Remit Token: 40 - Wallet
                            /// SC: Token Redeem: 41 - Wallet
                            if ((sc == "00") || (sc == "01") || (sc == "30") || (sc == "31") || (sc == "32")
                            || (sc == "50") || (sc == "51") || (sc == "40") || (sc == "41"))
                            {
                                //Transaction Limit Wallet
                                DataTable dtTransWalletLimit = WalletLimitUtils.GetWalletLimit(mobile, sc);
                                if (dtTransWalletLimit.Rows.Count > 0)
                                {
                                    DailyLimit = Convert.ToDecimal(dtTransWalletLimit.Rows[0]["WPerDayAmt"].ToString() == "" ? "0" : dtTransWalletLimit.Rows[0]["WPerDayAmt"].ToString());
                                    MonthlyLimit = Convert.ToDecimal(dtTransWalletLimit.Rows[0]["WTxnAmtM"].ToString() == "" ? "0" : dtTransWalletLimit.Rows[0]["WTxnAmtM"].ToString());
                                    singleTransLimit = Convert.ToDecimal(dtTransWalletLimit.Rows[0]["WPerTxnAmt"].ToString() == "" ? "0" : dtTransWalletLimit.Rows[0]["WPerTxnAmt"].ToString());
                                    transCount = Convert.ToInt32(dtTransWalletLimit.Rows[0]["WTxnCount"].ToString() == "" ? "0" : dtTransWalletLimit.Rows[0]["WTxnCount"].ToString());
                                }

                                DataTable dtableUserWalletLimit = new DataTable();
                                dtableUserWalletLimit = WalletLimitUtils.GetWalletUserLimitInfo(mobile);
                                if (dtableUserWalletLimit != null)
                                {
                                    if (dtableUserWalletLimit.Rows.Count == 0)
                                    {
                                        totalAmount = "0";
                                    }
                                    else if (dtableUserWalletLimit.Rows.Count > 0)
                                    {
                                        totalAmount = dtableUserWalletLimit.Rows[0]["Amount"].ToString();
                                        totalCount = dtableUserWalletLimit.Rows[0]["TotalCount"].ToString();
                                        balance = dtableUserWalletLimit.Rows[0]["Balance"].ToString();
                                        totalMonthlyTran = dtableUserWalletLimit.Rows[0]["TotalMonthlyWTran"].ToString();

                                        if (totalAmount == "")
                                        {
                                            totalAmount = "0";
                                        }
                                        if (totalMonthlyTran == "")
                                        {
                                            totalMonthlyTran = "0";
                                        }
                                    }

                                    totTxnAmt = decimal.Parse(totalAmount);
                                    totMonthlyTxnAmt = decimal.Parse(totalMonthlyTran);

                                    baln = decimal.Parse(balance);
                                }
                                else
                                {
                                    statusCode = "400";
                                    message = "Insufficient Balance";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                            }
                            /// 10 - Bank to Wallet 11- Bank to Bank 33 - B2B Merchant Payment 34 - Bank B2B Utility Payment 35 - B2B Coupon Recharge
                            if ((sc == "10") || (sc == "11") || (sc == "33") || (sc == "34") || (sc == "35"))
                            {
                                //Transaction Limit Bank
                                DataTable dtTransBankLimit = WalletLimitUtils.GetLimit(mobile);
                                if (dtTransBankLimit.Rows.Count > 0)
                                {
                                    DailyLimit = Convert.ToDecimal(dtTransBankLimit.Rows[0]["PerDayAmt"].ToString() == "" ? "0" : dtTransBankLimit.Rows[0]["PerDayAmt"].ToString());
                                    MonthlyLimit = Convert.ToDecimal(dtTransBankLimit.Rows[0]["TxnAmtM"].ToString() == "" ? "0" : dtTransBankLimit.Rows[0]["TxnAmtM"].ToString());
                                    singleTransLimit = Convert.ToDecimal(dtTransBankLimit.Rows[0]["PerTxnAmt"].ToString() == "" ? "0" : dtTransBankLimit.Rows[0]["PerTxnAmt"].ToString());
                                    transCount = Convert.ToInt32(dtTransBankLimit.Rows[0]["TxnCount"].ToString() == "" ? "0" : dtTransBankLimit.Rows[0]["TxnCount"].ToString());
                                }

                                DataTable dtableUserBankLimit = new DataTable();
                                dtableUserBankLimit = WalletLimitUtils.GetBankUserLimitInfo(mobile);
                                if (dtableUserBankLimit != null)
                                {
                                    if (dtableUserBankLimit.Rows.Count == 0)
                                    {
                                        totalAmount = "0";
                                    }
                                    else if (dtableUserBankLimit.Rows.Count > 0)
                                    {
                                        totalAmount = dtableUserBankLimit.Rows[0]["Amount"].ToString();
                                        totalCount = dtableUserBankLimit.Rows[0]["TotalCount"].ToString();
                                        totalMonthlyTran = dtableUserBankLimit.Rows[0]["TotalMonthlyBTran"].ToString();

                                        if (totalAmount == "")
                                        {
                                            totalAmount = "0";
                                        }
                                        if (totalMonthlyTran == "")
                                        {
                                            totalMonthlyTran = "0";
                                        }
                                    }

                                    totTxnAmt = decimal.Parse(totalAmount);
                                    totMonthlyTxnAmt = decimal.Parse(totalMonthlyTran);
                                    amtInt32 = decimal.Parse(amount);
                                }
                                else
                                {
                                    statusCode = "400";
                                    message = "Insufficient Balance";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }

                                //Individual Transaction Limit Enable
                                DataTable dtIndvTransLimit = WalletLimitUtils.GetIndividualLimit(mobile);
                                if (dtIndvTransLimit.Rows.Count > 0)
                                {
                                    EDIndvTxn = Convert.ToString(dtIndvTransLimit.Rows[0]["IndvTxn"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["IndvTxn"].ToString());
                                    DateRange = Convert.ToString(dtIndvTransLimit.Rows[0]["DateRange"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["DateRange"].ToString());
                                    StartDate = Convert.ToString(dtIndvTransLimit.Rows[0]["StartDate"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["StartDate"].ToString());
                                    EndDate = Convert.ToString(dtIndvTransLimit.Rows[0]["EndDate"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["EndDate"].ToString());
                                    IndvTransLimit = Convert.ToInt32(dtIndvTransLimit.Rows[0]["TransactionLimit"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["TransactionLimit"].ToString());

                                    LimitType = Convert.ToString(dtIndvTransLimit.Rows[0]["LimitType"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["LimitType"].ToString());
                                    IndvTransCount = Convert.ToInt32(dtIndvTransLimit.Rows[0]["TransactionCount"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["TransactionCount"].ToString());
                                    IndvTransLimitMonthly = Convert.ToInt32(dtIndvTransLimit.Rows[0]["TransactionLimitMonthly"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["TransactionLimitMonthly"].ToString());
                                    IndvTransLimitDaily = Convert.ToInt32(dtIndvTransLimit.Rows[0]["TransactionLimitDaily"].ToString() == "" ? "0" : dtIndvTransLimit.Rows[0]["TransactionLimitDaily"].ToString());
                                }

                                //Total INDV Transaction Limit
                                DataTable dtableUserIndvBankLimit = new DataTable();
                                dtableUserIndvBankLimit = WalletLimitUtils.GetBankUserIndvTotalLimitInfo(mobile);
                                if (dtableUserIndvBankLimit != null)
                                {
                                    if (dtableUserIndvBankLimit.Rows.Count == 0)
                                    {
                                        totalIndvDailyAmount = "0";
                                        totalIndvMonthlyAmount = "0";
                                    }
                                    else if (dtableUserIndvBankLimit.Rows.Count > 0)
                                    {
                                        totalIndvDailyAmount = dtableUserIndvBankLimit.Rows[0]["TotalDailyIndvTxnAmt"].ToString();
                                        totalCount = dtableUserBankLimit.Rows[0]["TotalCount"].ToString();
                                        totalIndvMonthlyAmount = dtableUserIndvBankLimit.Rows[0]["TotalMonthlyIndvTxnAmt"].ToString();

                                        if (totalIndvDailyAmount == "")
                                        {
                                            totalIndvDailyAmount = "0";
                                        }
                                        if (totalIndvMonthlyAmount == "")
                                        {
                                            totalIndvMonthlyAmount = "0";
                                        }
                                    }

                                    totIndvTxnDailyAmt = decimal.Parse(totalIndvDailyAmount);
                                    totIndvTxnMonthlyAmt = decimal.Parse(totalIndvMonthlyAmount);
                                }

                            }

                            //For Indv
                            totIndvDailyAmt = (totIndvTxnDailyAmt) + decimal.Parse(amount);
                            totIndvMonthlyAmt = (totIndvTxnMonthlyAmt) + decimal.Parse(amount);

                            //Total FOR Profile LIMIT
                            totAmt = (totTxnAmt) + decimal.Parse(amount);

                            totMonthlyAmt = (totMonthlyTxnAmt) + decimal.Parse(amount);

                            //START CHECK LIMIT WALLET
                            if ((sc == "00") || (sc == "01") || (sc == "30") || (sc == "31") || (sc == "32")
                            || (sc == "50") || (sc == "51") || (sc == "40") || (sc == "41"))
                            {
                                ////CHECK LIMIT
                                if (totMonthlyAmt > MonthlyLimit)
                                {
                                    statusCode = "508";
                                    message = "Monthly Transaction amount limit reached. ";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (Convert.ToInt32(totalCount) > transCount)
                                {
                                    if (totAmt > DailyLimit)
                                    {
                                        statusCode = "508";
                                        message = "Daily Transaction count limit reached.";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                }
                                if (totAmt > DailyLimit)
                                {
                                    statusCode = "508";
                                    message = "Daily Transaction amount limit reached. ";
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (Decimal.Parse(amount) > singleTransLimit)
                                {
                                    statusCode = "508";
                                    message = "Cannot perform Transaction exceeding " + singleTransLimit.ToString("#,##0.00");
                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                }
                                if (baln < amtInt32)
                                {
                                    if (statusCode != "")
                                    {
                                        statusCode = "400";
                                        String msg = message;
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                    else
                                    {
                                        statusCode = "500";
                                        message = "Insufficient Balance";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }

                                }

                                ////END CHECK LIMIT
                            }
                            ////END CHECK LIMIT WALLET
                            //START CHECK LIMIT BANK
                            if ((sc == "10") || (sc == "11") || (sc == "33") || (sc == "34") || (sc == "35"))
                            {
                                //START CHECK LIMIT INDV
                                if (EDIndvTxn == "En")
                                {
                                    //CHECK LIMIT INDV
                                    DateTime todayDate = DateTime.Today;
                                    string dateNow = Convert.ToString(todayDate);

                                    //Change Pattern
                                    string pattern = "d/M/yyyy";
                                    DateTime sdt = DateTime.ParseExact(StartDate, pattern, CultureInfo.InvariantCulture);

                                    DateTime dt2 = DateTime.ParseExact(EndDate, "d/M/yyyy", CultureInfo.InvariantCulture);

                                    if ((todayDate >= sdt) && (DateTime.Today <= dt2))
                                    {
                                        if (LimitType == "L")
                                        {
                                            if (totIndvMonthlyAmt > IndvTransLimitMonthly)
                                            {
                                                decimal mthlimitRemain = IndvTransLimitMonthly - totIndvMonthlyAmt;
                                                statusCode = "508";
                                                message = "Individual Transaction amount limit reached. Remaing Amount: " + mthlimitRemain;
                                                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                            }
                                            if (Convert.ToInt32(totalCount) > IndvTransCount)
                                            {
                                                if (totIndvDailyAmt > IndvTransLimitDaily)
                                                {
                                                    decimal indvlimitRemain = IndvTransLimitDaily - totIndvDailyAmt;
                                                    statusCode = "508";
                                                    message = "Daily Transaction count limit reached. Remining Amount: " + indvlimitRemain;
                                                    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                                }
                                            }
                                            if (totIndvDailyAmt > IndvTransLimitDaily)
                                            {
                                                decimal indvlimitRemain = IndvTransLimitDaily - totIndvDailyAmt;
                                                statusCode = "508";
                                                message = "Daily Individual Transaction amount limit reached. Remaining Amount: " + indvlimitRemain;
                                                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                            }
                                            if (Decimal.Parse(amount) > IndvTransLimit)
                                            {
                                                statusCode = "508";
                                                message = "Cannot perform Transaction exceeding " + IndvTransLimit.ToString("#,##0.00");
                                                replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                            }
                                            //if (baln < amtInt32)
                                            //{
                                            //    statusCode = "500";
                                            //    message = "Insufficient Balance";
                                            //    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                            //}

                                        }
                                        if (LimitType == "U")
                                        {
                                            //if (baln < amtInt32)
                                            //{
                                            //    statusCode = "500";
                                            //    message = "Insufficient Balance";
                                            //    replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                            //}
                                        }

                                    }
                                    else
                                    {
                                        statusCode = "500";
                                        message = "Individual Limit is Expired";
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }

                                }
                                //END CHECK LIMIT INDV
                                if (EDIndvTxn == "Dis")
                                {
                                    ////CHECK PROFILE LIMIT
                                    if (totMonthlyAmt > MonthlyLimit)
                                    {
                                        statusCode = "508";
                                        decimal mthlimitRemain = MonthlyLimit - totMonthlyAmt;
                                        message = "Monthly Transaction amount limit reached. Remaining monthly Limit: " + mthlimitRemain;
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                    if (Convert.ToInt32(totalCount) > transCount)
                                    {
                                        if (totAmt > DailyLimit)
                                        {
                                            statusCode = "508";
                                            decimal dailyLimit = DailyLimit - totAmt;
                                            message = "Daily Transaction count limit reached. Remain daily limit: " + dailyLimit;
                                            replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                        }
                                    }
                                    if (totAmt > DailyLimit)
                                    {
                                        decimal dailyLimit = DailyLimit - totAmt;
                                        statusCode = "508";
                                        message = "Daily Transaction amount limit reached. Remain daily limit: " + dailyLimit;
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }
                                    if (Decimal.Parse(amount) > singleTransLimit)
                                    {
                                        statusCode = "508";
                                        message = "Cannot perform Transaction exceeding " + singleTransLimit.ToString("#,##0.00");
                                        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    }

                                    //if (baln < amtInt32)
                                    //{
                                    //    if (statusCode != "")
                                    //    {
                                    //        statusCode = statusCode.ToString(); //"400";
                                    //        String msg = message;
                                    //        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    //    }
                                    //    else
                                    //    {
                                    //        statusCode = "500";
                                    //        message = "Insufficient Balance";
                                    //        replyMessage.ResponseStatus(HttpStatusCode.InternalServerError, message);
                                    //    }
                                    //}

                                    ////END CHECK PROFILE LIMIT
                                }

                            }
                            //END CHECK LIMIT BANK
                        }
                    }
                }
                else
                {
                    statusCode = "400";
                    message = "Invalid pin ";
                    replyMessage.Response = "Invalid pin ";
                    replyMessage.ResponseCode = HttpStatusCode.Unauthorized.ToString();
                    replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
                }

            }

            else
            {
                statusCode = "400";
                message = "UnAuthorized User";
                replyMessage.Response = "UnAuthorized User";
                replyMessage.ResponseStatus(HttpStatusCode.Unauthorized, replyMessage.Response);
            }

            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var v = new
                {
                    StatusCode = "200",
                    StatusMessage = "Success",
                    BankBaln = BankBaln

                };
                result = JsonConvert.SerializeObject(v);
            }
            else if ((response.StatusCode == HttpStatusCode.Unauthorized) || (response.StatusCode == HttpStatusCode.InternalServerError) 
                || (response.StatusCode == HttpStatusCode.BadRequest))
            {
                var v = new
                {
                    StatusCode = statusCode,
                    StatusMessage = message,
                    BankBaln = ""
                };
                result = JsonConvert.SerializeObject(v);
            }

            return result;
        }

       

    }

}
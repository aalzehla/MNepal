using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.DAL;


namespace MNepalProject.Services
{
    public class Pin:ReplyMessage
    {
        string PinChanged = "";

        public string ChangePIN(string mobile, string pin, string newpin)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());

            try
            {
                MNClientContact mn = new MNClientContact();
                mn.ContactNumber1 = mobile;
                ClientsDetails clientdetails=new ClientsDetails(mn);
                if (clientdetails.clientExt != null && clientdetails.clientContact!=null)
                {
                    if (clientdetails.clientExt.PIN == pin && clientdetails.client.Status=="Active")
                    {

                        //dataContext.Execute("UPDATE MNClient SET PIN= @0 WHERE ClientCode = @1 and PIN=@2", newpin, clientdetails.client.ClientCode, pin);
                        dataContext.Update<MNClientExt>("SET PIN=@0 WHERE ClientCode=@1 and PIN=@2",newpin,clientdetails.client.ClientCode,pin);
                        PinChanged = "true";
                    }
                    else
                    {
                        PinChanged = "false";
                    }
                }
                else
                {
                    PinChanged = "false";
                }

            }
            catch (Exception ex)
            {
                PinChanged = "false";
            }

            return PinChanged;

        }
        public bool validPIN(string mobile, string PIN)
        {
            bool result = false;
            MNClientContact mnClientContact = new MNClientContact(null, mobile, null);
            ClientsDetails ClientDetails = new ClientsDetails(mnClientContact);
            if (ClientDetails.clientExt != null)
            {
                if (ClientDetails.clientExt.PIN == PIN)
                {
                    result = true;
                }
            }
            return result;
        }

        public string GeneratePin()
        {

            var chars = "0123456789";
            var stringChars = new char[4];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var pin = new String(stringChars);
            return pin;
        }

        public string ScrambleWord(string word)
        {
            //
            Random rand = new Random();
            int wordLength = word.Length;
            if (wordLength != 10)
            {
                int remainingLength = 10 - wordLength;
                for (int i = 0; i < remainingLength; i++)
                {
                    string getChars = GenerateRandomCharacters();
                    word = getChars + word;
                }

            }
            else
            {

            }

            string temp = word;
            string result = string.Empty;

            for (int a = 0; a < word.Length; a++)
            {
                //multiplied by a number to get a better result, it was less likely for the last index to be picked
                int temp1 = rand.Next(0, (temp.Length - 1) * 3);

                result += temp[temp1 % temp.Length];
                temp = temp.Remove(temp1 % temp.Length, 1);
            }

            return result;
        }

        private string GenerateRandomCharacters()
        {
            Random rand = new Random();
            string specialCharacters = "109AzYG";
            char bb = specialCharacters[rand.Next(0, specialCharacters.Length)];

            string Chars = bb.ToString();
            return Chars;
        }

        /***START FORGOT PIN***/
        /// <summary>
        /// FORGOT PIN
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="newpin"></param>
        /// <returns></returns>
        public string ForgotPIN(string mobile, string newpin)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());

            try
            {
                MNClientContact mn = new MNClientContact();
                mn.ContactNumber1 = mobile;
                ClientsDetails clientdetails = new ClientsDetails(mn);
                if (clientdetails.clientExt != null && clientdetails.clientContact != null)
                {
                    if (clientdetails.client.Status == "Active")
                    {
                        dataContext.Update<MNClientExt>("SET PIN=@0 WHERE ClientCode=@1 ", newpin, clientdetails.client.ClientCode);
                        PinChanged = "true";
                    }
                    else
                    {
                        PinChanged = "false";
                    }
                }
                else
                {
                    PinChanged = "false";
                }

            }
            catch (Exception ex)
            {
                PinChanged = "false";
            }

            return PinChanged;

        }

        /***END FORGOT PIN***/

        /***START RESET PIN***/
        /// <summary>
        /// RESET PIN
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="newpin"></param>
        /// <returns></returns>
        public string PINReset(string mobile, string newpin)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());

            try
            {
                MNClientContact mn = new MNClientContact();
                mn.ContactNumber1 = mobile;
                ClientsDetails clientdetails = new ClientsDetails(mn);
                if (clientdetails.clientExt != null && clientdetails.clientContact != null)
                {
                    if (clientdetails.client.Status == "Active")
                    {
                        dataContext.Update<MNClientExt>("SET PIN=@0 WHERE ClientCode=@1 ", newpin, clientdetails.client.ClientCode);
                        dataContext.Update<MNPinLog>("SET PinChanged='T' WHERE ClientCode=@0 ", clientdetails.client.ClientCode);
                        PinChanged = "true";
                    }
                    else
                    {
                        PinChanged = "false";
                    }
                }
                else
                {
                    PinChanged = "false";
                }

            }
            catch (Exception ex)
            {
                PinChanged = "false";
            }

            return PinChanged;

        }

        /***END RESET PIN***/

    }
}
    

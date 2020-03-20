using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MNepalProject.Models;
using MNepalProject.Controllers;

namespace MNepalProject.Services
{
    public class ClientsDetails
    {
        public MNClient client { get; private set; }
        public MNClientContact clientContact { get; private set; }

        public MNAccountInfo accountInfo { get; private set; }

        public IEnumerable<MNAccountInfo> accountInfos { get; private set; } //wallets

        public MNBankAccountMap bankAccountMap { get; private set; }

        public IEnumerable<MNBankAccountMap> bankAccountMaps { get; private set; } //bank accounts

        public MNSubscribedProduct subscribedActiveProduct { get; private set; } //active default product suscribed by client
        public IEnumerable<MNSubscribedProduct> subscribedProducts { get; private set; } //suscribed products
        public IEnumerable<MNSubscribedProduct> subscribedActiveProducts { get; private set; } //suscribed active products


        public MNClientExt clientExt;


        public ClientsDetails(MNClient client)
        {
            this.ClientsDetailsByClient(client);
        }

        public ClientsDetails(MNClientContact clientContact)
        {
            this.ClientsDetailsByClientContact(clientContact);
        }

        public ClientsDetails(MNClientExt clientContact)
        {
            this.ClientsDetailsByClientExts(clientContact);
        }

        private void ClientsDetailsByClient(MNClient client)
        {
            //getting client by clientcode
            MNClientsController mnClientsController = new MNClientsController();
            this.client = mnClientsController.ClientByCode(client.ClientCode);

            if (this.client != null)
            {
                //getting client contact by clientcode
                MNClientContactsController mnClientsContactController = new MNClientContactsController();
                this.clientContact = mnClientsContactController.ClientContactByCode(this.client.ClientCode);


                //getting client ext details by clientcode
                MNClientExtsController mnClientsExtController = new MNClientExtsController();
                this.clientExt = mnClientsExtController.ClientExtsByCode(this.client.ClientCode);

                //getting client all account info by client code
                MNAccountInfosController mnAccountInfosController = new MNAccountInfosController();
                this.accountInfos = mnAccountInfosController.ClientAccountInfosByCode(this.client.ClientCode);
                //getting client default accountInfo by client code
                this.accountInfo = mnAccountInfosController.ClientDefaultAccountInfoByCode(this.client.ClientCode);

                //getting client all bank account info by client code
                MNBankAccountMapsController mnbankAccountMapsController = new MNBankAccountMapsController();
                this.bankAccountMaps = mnbankAccountMapsController.ClientBankAccountMapsByCode(this.client.ClientCode);
                //getting client default bankAccountMap
                this.bankAccountMap = mnbankAccountMapsController.ClientDefaultBankAccount(this.client.ClientCode);

                //getting client all suscribed products by client code
                MNSubscribedProductsController mnSuscribedProductsController = new MNSubscribedProductsController();
                this.subscribedProducts = mnSuscribedProductsController.ClientSuscribedProductsByCode(this.client.ClientCode);
                this.subscribedActiveProducts = mnSuscribedProductsController.ClientSuscribedActiveProductsByCode(this.client.ClientCode);
                //getting client default suscribed product by client code
                this.subscribedActiveProduct = mnSuscribedProductsController.ClientDefaultSuscribedActiveProductByCode(this.client.ClientCode);
            }
        }
        private void ClientsDetailsByClientContact(MNClientContact clientContact)
        {
            //getting client contact by contact numbers
            MNClientContactsController mnClientContactsController = new MNClientContactsController();
            this.clientContact = mnClientContactsController.ClientContactByContactNumber(clientContact.ContactNumber1);
            if (this.clientContact != null)
            {
                MNClient rawclient = new MNClient();
                rawclient.ClientCode = this.clientContact.ClientCode;
                //filling all details after getting client code
                this.ClientsDetailsByClient(rawclient);
            }
        }


        private void ClientsDetailsByClientExts(MNClientExt clientContact)
        {
            //getting client contact by contact numbers
            MNClientExtsController mnClientExtsController = new MNClientExtsController();
            this.clientExt = mnClientExtsController.ClientExtByContactNumber(clientContact.UserName);
            if (this.clientExt != null)
            {
                MNClient rawclient = new MNClient();
                rawclient.ClientCode = this.clientExt.ClientCode;
                //filling all details after getting client code
                this.ClientsDetailsByClient(rawclient);
            }
        }

       /* public ClientsDetails(MNClientExt clientExt)
        {
            this.clientExt = clientExt;
        }
        * */

        /*
           public string ClientCode { get; set; } //should be addressed by changing table structure.
        public string Name { get; set; }
        public string Address { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(4)]
        public string PIN { get; set; }
         
         */

        public MNClient getClient(string ClientCode)
        {
            return this.client;
        }
    }
}
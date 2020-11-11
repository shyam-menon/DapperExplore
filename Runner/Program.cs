using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DataLayer;
using Microsoft.Extensions.Configuration;

namespace Runner
{
    class Program
    {
        private static IConfigurationRoot config;
        static void Main(string[] args)
        {
            //Initialize the connection string
            Initialize();

            //1. Get
            // Get_all_should_return_6_results();

            //2. Insert
            var id = Insert_should_assign_identity_to_new_entity();
            //3. Find a record
            Find_should_retrieve_existing_entity(id);
            //4. Update the record
            Modify_should_update_existing_entity(id);
            //5. Delete the record
            Delete_should_remove_entity(id);

        }

        //1. Get
        static void Get_all_should_return_6_results()
        {
            // arrange
            var repository = CreateRepository();

            // act
            var contacts = repository.GetAll();

            // assert
            Console.WriteLine($"Count: {contacts.Count}");
            Debug.Assert(contacts.Count == 6);
            contacts.Output();
        }

        //2. Insert
        static int Insert_should_assign_identity_to_new_entity()
        {
            // arrange
            IContactRepository repository = CreateRepository();
            var contact = new Contact
            {
                FirstName = "Joe",
                LastName = "Blow",
                Email = "joe.blow@gmail.com",
                Company = "Microsoft",
                Title = "Developer"
            };
            var address = new Address
            {
                AddressType = "Home",
                StreetAddress = "123 Main Street",
                City = "Baltimore",
                StateId = 1,
                PostalCode = "22222"
            };
            contact.Addresses.Add(address);

            // act
            repository.Add(contact);
            //repository.Save(contact);

            // assert
            Debug.Assert(contact.Id != 0);
            Console.WriteLine("*** Contact Inserted ***");
            Console.WriteLine($"New ID: {contact.Id}");
            return contact.Id;
        }

        //3. Find a single record
        static void Find_should_retrieve_existing_entity(int id)
        {
            // arrange
            IContactRepository repository = CreateRepository();

            // act
            var contact = repository.Find(id);
            //var contact = repository.GetFullContact(id);

            // assert
            Console.WriteLine("*** Get Contact ***");
            contact.Output();
            Debug.Assert(contact.FirstName == "Joe");
            Debug.Assert(contact.LastName == "Blow");
            //Debug.Assert(contact.Addresses.Count == 1);
            //Debug.Assert(contact.Addresses.First().StreetAddress == "123 Main Street");
        }

        //4. Modify a record
        static void Modify_should_update_existing_entity(int id)
        {
            // arrange
            IContactRepository repository = CreateRepository();

            // act
            var contact = repository.Find(id);
            //var contact = repository.GetFullContact(id);
            contact.FirstName = "Bob";
            //contact.Addresses[0].StreetAddress = "456 Main Street";
            repository.Update(contact);
            //repository.Save(contact);

            // create a new repository for verification purposes
            IContactRepository repository2 = CreateRepository();
            var modifiedContact = repository2.Find(id);
            //var modifiedContact = repository2.GetFullContact(id);

            // assert
            Console.WriteLine("*** Contact Modified ***");
            modifiedContact.Output();
            Debug.Assert(modifiedContact.FirstName == "Bob");
           // Debug.Assert(modifiedContact.Addresses.First().StreetAddress == "456 Main Street");
        }

        //5. Delete a record
        static void Delete_should_remove_entity(int id)
        {
            // arrange
            IContactRepository repository = CreateRepository();

            // act
            repository.Remove(id);

            // create a new repository for verification purposes
            IContactRepository repository2 = CreateRepository();
            var deletedEntity = repository2.Find(id);

            // assert
            Debug.Assert(deletedEntity == null);
            Console.WriteLine("*** Contact Deleted ***");
        }

        private static void Initialize()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config = builder.Build();
        }

        private static IContactRepository CreateRepository()
        {
            return new ContactRepository(config.GetConnectionString("DefaultConnection"));
            //return new ContactRepositoryContrib(config.GetConnectionString("DefaultConnection"));
        }
    }
}

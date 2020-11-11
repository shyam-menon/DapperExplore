using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace DataLayer
{
    public class Contact
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }

        //Without the two attributes on the properties below, Dapper contrib throws an exception as
        //it tries to generate SQL and map these properties to the DB
        //indicates to Dapper contrib that this is to be ignored when generating SQL
        [Computed]
        public bool IsNew => this.Id == default(int);

        //indicates to Dapper contrib that SQL generation should not be done to insert this property
        //as there is no actual column called addresses.
        [Write(false)]
        public List<Address> Addresses { get; } = new List<Address>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneBook.Models
{
	public class PersonModel
	{
		[Required]
		public int ID { get; set; }

		[Required, MaxLength(32), DisplayName("Imię")]
		public string FirstName { get; set; }

		[Required, MaxLength(64), DisplayName("Nazwisko")]
		public string LastName { get; set; }

		[Required, MaxLength(24), DisplayName("Numer telefonu")]
		public string Phone { get; set; }

		[EmailAddress, DisplayName("Adres email")]
		public string Email { get; set; }

		public DateTime Created { get; set; }
		public DateTime? Updated { get; set; }

		/// <summary>
		/// 	Główny konstruktor - Wszystkie parametry
		/// </summary>
		/// <param name="iD"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="phone"></param>
		/// <param name="email"></param>
		/// <param name="created"></param>
		/// <param name="updated"></param>

		public PersonModel(int iD, string firstName, string lastName, string phone, string email, DateTime created, DateTime? updated)
		{
			ID = iD;
			FirstName = firstName;
			LastName = lastName;
			Phone = phone;
			Email = email;
			Created = created;
			Updated = updated;
		}

		/// <summary>
		/// 	Konstruktor bezparametrowy - dla formularza
		/// </summary>

		public PersonModel()
		{
			Created = DateTime.Now;
			Updated = null;
		}
	}
}

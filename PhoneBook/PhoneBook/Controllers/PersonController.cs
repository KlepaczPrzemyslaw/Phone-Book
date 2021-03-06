﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Helpers;
using PhoneBook.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PhoneBook.Controllers
{
	public class PersonController : Controller
	{
		/// <summary>
		/// 	Index
		/// </summary>

		[HttpGet]
		public IActionResult Index(int id = 1, string searchText = null)
		{
			// Pobranie ogólnej liczby osób
			int count = string.IsNullOrWhiteSpace(searchText) ? SourceManager.GetCount() : SourceManager.GetCount(searchText);

			// Przypisanie szukanego tekstu do ViewBag-a
			ViewBag.SearchText = searchText;

			// Dla mniej niż 10 osób - tylko 1 strona
			if (id <= 1 && (id * 10) >= count)
			{
				ViewBag.Previous = null;
				ViewBag.PreviousDis = "disabled";
				ViewBag.Next = null;
				ViewBag.NextDis = "disabled";
				ViewBag.Page = 1;
			}
			// Dla pierwszej strony
			else if (id <= 1)
			{
				ViewBag.Previous = null;
				ViewBag.PreviousDis = "disabled";
				ViewBag.Next = 2;
				ViewBag.NextDis = "";
				ViewBag.Page = id;
			}
			// Dla ostatniej strony
			else if ((id * 10) >= count)
			{
				ViewBag.Previous = id - 1;
				ViewBag.PreviousDis = "";
				ViewBag.Next = null;
				ViewBag.NextDis = "disabled";
				ViewBag.Page = id;
			}
			// Dla strony pomiędzy
			else
			{
				ViewBag.Previous = id - 1;
				ViewBag.PreviousDis = "";
				ViewBag.Next = id + 1;
				ViewBag.NextDis = "";
				ViewBag.Page = id;
			}

			// Zwrócenie modelu z listą osób
			List <PersonModel> list = string.IsNullOrWhiteSpace(searchText) ? SourceManager.Get(id, 10) : SourceManager.Get(id, 10, searchText);

			// Zwrócenie widoku
			return View(list);
		}

		/// <summary>
		/// 	Add
		/// </summary>

		[HttpGet]
		public IActionResult Add()
		{
			// Zwrócenie widoku
			return View();
		}

		[HttpPost]
		public IActionResult Add(PersonModel personModel)
		{
			// Walidacja
			if (ModelState.IsValid)
			{
				// Dodanie w bazie
				SourceManager.Add(personModel);
				// Zwrócenie informacji
				TempData["alertclass"] = "alert alert-success my-3";
				TempData["message"] = "Dodano!";
				// Przekierowanie widoku
				return Redirect("/Person/Index");
			}
			// Zwrócenie widoku przy błędach
			return View(personModel);
		}

		/// <summary>
		/// 	Edit
		/// </summary>

		[HttpGet]
		public IActionResult Edit(int id)
		{
			// Pobranie osoby z bazy po ID
			PersonModel personModel = SourceManager.GetPerson(id);

			// Zwrócenie błędu - jeżeli id np. spoza zakresu
			if (personModel == null)
				return StatusCode(404);

			// Zwrócenie widoku
			return View(personModel);
		}

		[HttpPost]
		public IActionResult Edit(PersonModel personModel)
		{ 
			// Walidacja
			if (ModelState.IsValid)
			{
				// Edycja w bazie
				SourceManager.Edit(personModel);
				// Zwrócenie informacji
				TempData["alertclass"] = "alert alert-warning my-3";
				TempData["message"] = "Zmieniono!";
				// Przekierowanie widoku
				return Redirect("/Person/Index");
			}
			// Zwrócenie widoku przy błędach
			return View(personModel);
		}

		/// <summary>
		/// 	Remove
		/// </summary>

		[HttpGet]
		public IActionResult Remove(int id)
		{
			// Pobranie osoby z bazy po ID
			PersonModel personModel = SourceManager.GetPerson(id);

			// Zwrócenie błędu - jeżeli id np. spoza zakresu
			if (personModel == null)
				return StatusCode(404);

			// Zwrócenie widoku
			return View(personModel);
		}

		[HttpPost]
		public IActionResult Remove(PersonModel personModel)
		{
			// Usunięcie w bazie
			SourceManager.Remove(personModel);
			// Zwrócenie informacji
			TempData["alertclass"] = "alert alert-danger my-3";
			TempData["message"] = "Usunięto!";
			// Przekierowanie widoku
			return Redirect("/Person/Index");
		}
	}
}

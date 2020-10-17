﻿using System.Collections.Generic;
using System.Threading;
using Bogus;
using Nest6;
using Tests.Configuration;
using Tests.Domain.Helpers;

namespace Tests.Domain
{
	public class Person
	{
		protected static int IdState = 0;
		public string FirstName { get; set; }

		public static Faker<Person> Generator { get; } =
			new Faker<Person>()
				.UseSeed(TestConfiguration.Instance.Seed)
				.RuleFor(p => p.Id, p => Interlocked.Increment(ref IdState))
				.RuleFor(p => p.FirstName, p => p.Name.FirstName())
				.RuleFor(p => p.LastName, p => p.Name.LastName())
				.RuleFor(p => p.JobTitle, p => p.Name.JobTitle())
				.RuleFor(p => p.Location, p => new GeoLocation(Gimme.Random.Number(-90, 90), Gimme.Random.Number(-180, 180)));

		public long Id { get; set; }
		public string JobTitle { get; set; }
		public string LastName { get; set; }
		public GeoLocation Location { get; set; }

		public static IList<Person> People { get; } = Generator.Clone().Generate(1000);
	}
}

﻿using System;
using Elastic.Xunit.XunitPlumbing;
using FluentAssertions;
using Nest6;

namespace Tests.CodeStandards.Parity
{
	public class ParityTests
	{
		[U] public void FieldTypeHasAllNumberTypes()
		{
			var numberTypes = Enum.GetNames(typeof(NumberType));
			var fieldTypes = Enum.GetNames(typeof(FieldType));

			fieldTypes.Should().Contain(numberTypes);
		}
	}
}

﻿using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace HomeExercises
{
	[TestFixture]
	public class NumberValidatorTests
	{
		[Test]
		public void TestNumberValidator_Fails_OnInvalidNumberString()
		{
			using (new AssertionScope())
			{
				new NumberValidator(3, 2, true).IsValidNumber("+1.23").Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber("-1.23").Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber("00.00").Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber("-0.00").Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber("+0.00").Should().BeFalse();

				new NumberValidator(3, 2, true).IsValidNumber(" 1.23").Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber("1. 23").Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber("1.23 ").Should().BeFalse();

				new NumberValidator(3, 2, true).IsValidNumber(null).Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber(" ").Should().BeFalse();

				new NumberValidator(3, 1, true).IsValidNumber("1.24").Should().BeFalse();
				new NumberValidator(2, 2, true).IsValidNumber("1.24").Should().BeFalse();
				new NumberValidator(4).IsValidNumber("-1.23").Should().BeFalse();

				new NumberValidator(4, 2, true).IsValidNumber("-1.24").Should().BeFalse();
				new NumberValidator(4, 2, true).IsValidNumber(".24").Should().BeFalse();
				new NumberValidator(4, 2, true).IsValidNumber("1.").Should().BeFalse();
				new NumberValidator(4, 2, true).IsValidNumber(".").Should().BeFalse();

				new NumberValidator(4, 2, true).IsValidNumber("1..2").Should().BeFalse();
				new NumberValidator(4, 2, true).IsValidNumber("1.0.2").Should().BeFalse();
				new NumberValidator(4, 2, true).IsValidNumber("1.,2").Should().BeFalse();
				new NumberValidator(3, 2, true).IsValidNumber("a.sd").Should().BeFalse();
				new NumberValidator(4, 2, true).IsValidNumber("+-1.2").Should().BeFalse();
				new NumberValidator(4, 2, true).IsValidNumber("-+1.2").Should().BeFalse();
			}
		}

		[Test]
		public void TestNumberValidator_Passes_OnValidNumberString()
		{
			using (new AssertionScope())
			{
				new NumberValidator(17, 2, true).IsValidNumber("0.0").Should().BeTrue();
				new NumberValidator(17, 2, true).IsValidNumber("0").Should().BeTrue();
				new NumberValidator(17, 2, true).IsValidNumber("3214124").Should().BeTrue();
				new NumberValidator(17, 2, true).IsValidNumber("00.00").Should().BeTrue();
				new NumberValidator(4, 2, true).IsValidNumber("+1.23").Should().BeTrue();
				new NumberValidator(4, 2, true).IsValidNumber("1,23").Should().BeTrue();
				new NumberValidator(4, 2).IsValidNumber("-1.23").Should().BeTrue();
				new NumberValidator(4, 0, true).IsValidNumber("100").Should().BeTrue();
			}
		}

		[Test]
		public void TestNumberValidator_Throws_ExceptionsOnInvalidPrecisionOrScale()
		{
			using (new AssertionScope())
			{
				new Func<NumberValidator>(() => new NumberValidator(-1, 2, true)).Should().Throw<ArgumentException>();
				new Func<NumberValidator>(() => new NumberValidator(1, 2, true)).Should().Throw<ArgumentException>();
				new Func<NumberValidator>(() => new NumberValidator(1, -1, true)).Should().Throw<ArgumentException>();
			}
		}
	}

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale > precision)
				throw new ArgumentException("scale must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}
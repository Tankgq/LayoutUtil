using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtension {
	private static readonly List<char> SymbolList = new List<char> {
		'+', '-', '*', '/', '(', ')', '^', '#'
	};

	private static readonly Dictionary<char, int> SymbolPriorityInStackDic = new Dictionary<char, int> {
		{'+', 2},
		{'-', 2},
		{'*', 3},
		{'/', 3},
		{'(', 1},
		{')', 1},
		{'^', 4},
		{'#', 0}
	};
	
	private static readonly Dictionary<char, int> SymbolPriorityOutStackDic = new Dictionary<char, int> {
		{'+', 2},
		{'-', 2},
		{'*', 3},
		{'/', 3},
		{'(', 6},
		{')', 1},
		{'^', 5},
		{'#', 0}
	};
	
	private static double ParseDouble(string txt) {
		bool bSucceed = double.TryParse(txt, out double result);
		return bSucceed ? result : double.NaN;
	}

	private static bool Operating(Stack<char> symbolStack, Stack<double> numberStack, char operation) {
		while(true) {
			char operationInTop = symbolStack.Peek();
			if(operationInTop == '(' && operation == ')') {
				symbolStack.Pop();
				return true;
			}
			if(operationInTop == '#' && operation == '#') return true;
			if(SymbolPriorityInStackDic[operationInTop] < SymbolPriorityOutStackDic[operation]) {
				symbolStack.Push(operation);
				return true;
			}
			symbolStack.Pop();
			if(numberStack.Count < 2) return false;
			double lhs = numberStack.Pop();
			double rhs = numberStack.Pop();
			switch(operationInTop) {
				case '+':
					numberStack.Push(rhs + lhs);
					break;
				case '-':
					numberStack.Push(rhs - lhs);
					break;
				case '*':
					numberStack.Push(rhs * lhs);
					break;
				case '/':
					if(Math.Abs(lhs) < 0.0000000001) return false;
					numberStack.Push(rhs / lhs);
					break;
				case '^':
					numberStack.Push(Math.Pow(rhs, lhs));
					break;
				default:
					return false;
			}
		}
	}

	public static double Calculate(this string expression) {
		expression = expression.Trim();
		if(expression.Length == 0) return double.NaN;
		if(expression.IndexOf(' ') != -1) {
			StringBuilder sb = new StringBuilder();
			foreach(char ch in expression.Where(ch => ch != ' ')) sb.Append(ch);
			expression = sb.ToString();
		}
		Stack<char> symbolStack = new Stack<char>();
		symbolStack.Push('#');
		Stack<double> numberStack = new Stack<double>();
		int idx = 0, length = expression.Length;
		while(idx < length) {
			if(expression[idx] == '-') {
				if(idx + 1 >= length) return double.NaN;
				if(char.IsDigit(expression[idx + 1]) || expression[idx + 1] == '.') numberStack.Push(0.0);
			}
			if(expression[idx] == '^' && idx > 0 && expression[idx - 1] == '^') return double.NaN;
			bool isDot = expression[idx] == '.';
			if(isDot || char.IsDigit(expression[idx])) {
				int dotCount = isDot ? 1 : 0;
				int start = idx;
				++ idx;
				while(idx < length && ((isDot = expression[idx] == '.') || char.IsDigit(expression[idx]))) {
					if(isDot) ++ dotCount;
					++ idx;
				}
				if(dotCount > 1) return double.NaN;
				numberStack.Push(ParseDouble(expression.Substring(start, idx - start)));
				if(idx < length && expression[idx] == ')' && ! Operating(symbolStack, numberStack, expression[idx ++])) return double.NaN;
				if(! Operating(symbolStack, numberStack, idx < length ? expression[idx ++] : '#')) return double.NaN;
			} else if(! SymbolList.Contains(expression[idx])) {
				return double.NaN;
			} else {
				if(expression[idx] == ')')
					if(! Operating(symbolStack, numberStack, expression[idx ++])) return double.NaN;
				if(! Operating(symbolStack, numberStack, idx < length ? expression[idx ++] : '#')) return double.NaN;
			}
		}
		return numberStack.Count < 1 ? double.NaN : numberStack.Pop();
	}
	
	public static string CancelHighlight(this string text) {
		return string.IsNullOrEmpty(text) ? text : Regex.Replace(text, @"<color=[a-zA-Z]+><size=\d+><b>(?<str>.*?)</b></size></color>", @"${str}");
	}

	public static string HighlightText(this string text, string needHighlight) {
		if(string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(needHighlight)) return text;
		return text.Replace(needHighlight, "<color=yellow><size=25><b>" + needHighlight + "</b></size></color>");
	}
}

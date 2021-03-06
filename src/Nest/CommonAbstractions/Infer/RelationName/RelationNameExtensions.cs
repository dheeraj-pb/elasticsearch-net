﻿namespace Nest6
{
	internal static class RelationNameExtensions
	{
		internal static bool IsConditionless(this RelationName marker) => marker == null || marker.Name.IsNullOrEmpty() && marker.Type == null;
	}
}

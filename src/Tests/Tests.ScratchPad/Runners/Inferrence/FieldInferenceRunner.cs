﻿using Nest6;
using Tests.Domain;

namespace Tests.ScratchPad.Runners.Inferrence
{
	public class FieldInferenceRunner : RunBase
	{
		protected override int LoopCount => 1_000_000;

		protected override RoutineBase Routine() => Loop(() => Infer.Field<Project>(p => p.LeadDeveloper.LastName), (c, f) => c.Infer.Field(f));
	}
}

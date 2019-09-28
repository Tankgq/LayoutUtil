using System.Collections.Generic;

public static class BehaviorFactory {

	public static Behavior GetCreateModuleBehavior(string moduleName) {
		return new Behavior(isReDo => ModuleUtil.CreateModuleBehavior(moduleName,
																	  null,
																	  true,
																	  isReDo),
							isReUndo => ModuleUtil.RemoveModuleBehavior(moduleName),
							Behavior.BehaviorType.CreateModule);
	}

	public static Behavior GetRemoveModuleBehavior(string moduleName) {
		return new Behavior(isReDo => ModuleUtil.RemoveModuleBehavior(moduleName),
							isReUndo => ModuleUtil.CreateModuleBehavior(moduleName),
							Behavior.BehaviorType.RemoveModule);
	}

	public static Behavior GetRemoveAllModuleBehavior(List<string> modules, bool combineWithNextBehavior = false) {
		return new Behavior(isRedo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx)
									ModuleUtil.RemoveModuleBehavior(modules[idx]);
							},
							isReUndo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx)
									ModuleUtil.CreateModuleBehavior(modules[idx], null, false);
							},
							Behavior.BehaviorType.RemoveAllModule, combineWithNextBehavior);
	}

}

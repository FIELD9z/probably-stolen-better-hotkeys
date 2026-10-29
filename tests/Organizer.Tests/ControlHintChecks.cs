using BetterHotkeys;

internal static class ControlHintChecks
{
    internal static void Run(Action<string, bool> check)
    {
        var p = new Preferences();
        List<ControlHintRow> Rows(bool active = true, bool chinese = false) =>
            ControlHintRows.Build(active, p, chinese, id => "native:" + id, (_, _) => "");
        check("inactive mod does not generate replacement hints", Rows(false).Count == 0);
        var rows = Rows();
        check("takeover shows correct counter and container actions", rows[0].Key == "Shift + Click" && rows[0].Action.Contains("sales counter") && rows[1].Key == "Ctrl + Click" && rows[1].Action.Contains("selected owned"));
        check("takeover replaces old alt-transfer hint with pin", rows[2].Key == "Alt + Click" && rows[2].Action == "Pin / unpin");
        p.Counter = Preferences.Parse("Ctrl+Q");
        check("applied custom binding appears in hints", Rows()[0].Key == "Ctrl+Q + Click");
        p.Counter = Preferences.Parse("RightShift");
        check("equivalent modifiers shown generically", Rows()[0].Key == "Shift + Click");
        p.Equivalent = false;
        check("side-specific binding shown exactly", Rows()[0].Key == "RightShift + Click");
        p.Transfer = false;
        check("disabled transfer is not advertised as available", Rows()[0].Key == "Disabled" && Rows()[1].Key == "Disabled");
        p.PinKey = Preferences.Parse("None");
        check("cleared binding shown as unbound", Rows()[2].Key == "Unbound");
        check("following native sort reads current game binding", Rows()[3].Key == "native:sort_menu");
        p.FollowSort = false; p.SortKey = Preferences.Parse("F6");
        check("independent sort reads custom binding", Rows()[3].Key == "F6");
        p.Sort = false;
        check("disabled sorting shown as disabled", Rows()[3].Key == "Disabled");
        check("default marker gesture is double press", Rows()[4].Key == "Double B");
        p.MarkerKey = Preferences.Parse("Ctrl+B"); p.Trigger = "Hold";
        check("hold marker gesture follows custom chord", Rows()[4].Key == "Hold LeftCtrl+B");
        p.Trigger = "Press";
        check("single press marker hint", Rows()[4].Key == "Press LeftCtrl+B");
        p.Markers = false;
        check("disabled markers shown as disabled", Rows()[4].Key == "Disabled");
        p.Takeover = false;
        rows = Rows();
        check("vanilla mode uses native transfer bindings even when custom module is off", rows[0].Key == "native:quick_move_inventory + Click" && rows[1].Key == "native:quick_move_showcase + Click" && rows[2].Key == "native:quick_move_storage + Click");
        check("vanilla mode does not advertise custom pin gesture", rows.Take(3).All(r => r.Action.StartsWith("Vanilla:")));
        check("unrelated native actions retain current game bindings", rows[5].Key == "native:rotate_ccw" && rows[6].Key == "native:rotate_cw" && rows[7].Key == "native:flip_item" && rows[9].Key == "native:advance_dialogue");
        p.Takeover = true; p.Transfer = true; p.Equivalent = true;
        rows = ControlHintRows.Build(true, p, false, id => id, (_, _) => "occupied");
        check("known conflict is not advertised as usable", rows[0].Key == "Shift + Click [Conflict]");
        check("Chinese hints use current binding and translated action", Rows(true, true)[0].Key == "Shift + 左键" && Rows(true, true)[2].Key == "未绑定");
    }
}

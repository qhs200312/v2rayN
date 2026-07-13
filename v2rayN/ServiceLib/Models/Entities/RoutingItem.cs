namespace ServiceLib.Models.Entities;

[Serializable]
public class RoutingItem
{
    [PrimaryKey]
    public string Id { get; set; }

    public string Remarks { get; set; }
    public string Url { get; set; }
    public string RuleSet { get; set; }
    public int RuleNum { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Locked { get; set; }
    public string CustomIcon { get; set; }
    public string CustomRulesetPath4Singbox { get; set; }
    public string DomainStrategy { get; set; }
    public string DomainStrategy4Singbox { get; set; }
    public int Sort { get; set; }
    public bool IsActive { get; set; }

    public string DisplayRemarks => Remarks switch
    {
        "RUv1-Всё" => "RUv1-全部",
        "RUv1-Всё, кроме РФ" => "RUv1-除俄罗斯外",
        "RUv1-Заблокированное" => "RUv1-被屏蔽内容",
        _ => Remarks
    };
}

namespace Casbin.Watcher.Redis
{
    public enum MethodType
    {
        None,
        Update,
        UpdateForAddPolicy,
        UpdateForRemovePolicy,
        UpdateForRemoveFilteredPolicy,
        UpdateForSavePolicy,
        UpdateForAddPolicies,
        UpdateForRemovePolicies
    }
}
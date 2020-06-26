
public interface IPoolableObject
{
    void OnInit();
    void OnReset();
    void OnFree();
}

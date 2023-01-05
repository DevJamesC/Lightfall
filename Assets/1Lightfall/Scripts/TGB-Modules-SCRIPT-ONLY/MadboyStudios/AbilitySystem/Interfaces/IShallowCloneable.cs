
public interface IShallowCloneable<T> where T: new()
{
    T GetShallowCopy() { return new T(); }
}

using System.Collections.Generic;

public interface IDataAccess<T>
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    int Add(T entity);
    bool Update(T entity);
    bool Delete(int id);
}

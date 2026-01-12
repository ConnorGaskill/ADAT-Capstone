using System.Collections.Generic;

public interface IDataAccess<T>
{
    IEnumerable<T> GetAll();
    T GetById(int id);

    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

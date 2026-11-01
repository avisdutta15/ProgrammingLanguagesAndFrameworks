using System;

class Employee{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Salary { get; set; }
}

class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    Employee []empList;

    public Department()
    {        
        DepartmentId = 10;
        DepartmentName = "Sales";
        empList = new Employee[3]
        {
            new Employee{ Id = 100, Name = "A", Salary = 1000},
            new Employee{ Id = 101, Name = "B", Salary = 2000},
            new Employee{ Id = 102, Name = "C", Salary = 3000}
        };
    }

    public Employee GetEmployee(int id)
    {
        foreach(Employee emp in empList)
        {
            if (emp.Id == id)
                return emp;
        }
        return null;
    }

    public Employee GetEmployee(string name)
    {
        foreach (Employee emp in empList)
        {
            if (emp.Name == name)
                return emp;
        }
        return null;
    }

    public Employee this[int id]{
        get
        {
            foreach (Employee emp in empList)
            {
                if (emp.Id == id)
                    return emp;
            }
            return null;
        }
        set
        {

        }
    }

    public Employee this[string name]
    {
        get
        {
            foreach (Employee emp in empList)
            {
                if (emp.Name == name)
                    return emp;
            }
            return null;
        }
    }
}
    
class Program
{
    static void Main(string[] args)
    {
        Department dept = new Department();

        //As we have defined indexers in Department class
        //so its objects can be accessed using Array access operator []
        
        Console.WriteLine(dept[101].Name); //get the name of emp with id = 101
        Console.WriteLine(dept["C"].Id);   //get the id of emp with name = "C"
        Console.ReadLine();
    }
}
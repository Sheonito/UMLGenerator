namespace UMLAutoGenerator
{
    public class Singleton<T> where T : new() // T를 new로 생성하기 위해 T를 new()로 제한
    {
        protected static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new T();

                return instance;
            }
            private set { }
        }
    }
}

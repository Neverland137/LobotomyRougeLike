using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class HealthManager
    {
        private static HealthManager _instance;
        public static HealthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HealthManager();
                }
                return _instance;
            }
        }
        private int _health = 0;
        public void LoadData(Dictionary<string, object> dic)
        {
            GameUtil.TryGetValue<int>(dic, "health", ref _health);
        }
        public void ReadHealth()
        {
        }
        public int GetHealth()
        {
            return _health;
        }
        public void AddHealth(int amount)
        {
            _health += amount;
        }
        public void SetHealth(int amount)
        {
            _health = amount;
        }
    }
}

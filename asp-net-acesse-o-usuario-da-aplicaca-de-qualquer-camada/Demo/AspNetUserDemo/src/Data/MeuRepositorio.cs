using Business;
using Business.Interfaces;

namespace Data
{
    public class MeuRepositorio : IRepositorio
    {
        private readonly IUser _user;

        public MeuRepositorio(IUser user)
        {
            _user = user;
        }

        public void Adicionar(Autor autor)
        {
            var dadosDoSeuUser = _user.Name;
        }
    }
}
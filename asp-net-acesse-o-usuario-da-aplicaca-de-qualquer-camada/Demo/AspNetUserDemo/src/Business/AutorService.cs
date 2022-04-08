using Business.Interfaces;

namespace Business
{
    public class AutorService : IAutorService
    {
        private IRepositorio _meuRepositorio;


        public AutorService(IRepositorio meuRepositorio)
        {
            _meuRepositorio = meuRepositorio;
        }

        public void Adicionar(Autor autor)
        {
            _meuRepositorio.Adicionar(autor);
        }
    }
}

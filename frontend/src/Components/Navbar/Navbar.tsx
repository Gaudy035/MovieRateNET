import Logo from './subcomponents/Logo';
import NavButton from './NavButton';
import Searchbar from './subcomponents/Searchbar';

export default function Navbar() {
  return (
    <header className='bg-purple-900 sticky top-0'>
      <nav className='text-white flex justify-around items-center px-4 py-4'>
        <Logo />
        <Searchbar />
        <NavButton />
      </nav>
    </header>
  );
}

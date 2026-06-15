interface InputTempProps {
  type: string;
  placeholder: string;
  name: string;
  label: string;
}

export default function InputTemp({
  type,
  placeholder,
  name,
  label,
}: InputTempProps) {
  return (
    <div className='flex flex-col justify-center items-start gap-1'>
      <label htmlFor={name} className='px-2'>
        {label}:
      </label>
      <input
        type={type}
        name={name}
        id={name}
        placeholder={placeholder}
        required
        className='border rounded-xl py-1 px-3 focus:scale-105 transition duration-200'
      />
    </div>
  );
}

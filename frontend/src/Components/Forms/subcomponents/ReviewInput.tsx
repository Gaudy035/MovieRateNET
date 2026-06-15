interface ReviewInputProps {
  type: string;
  placeholder: string;
  name: string;
  label: string;
}

export default function ReviewInput({
  type,
  placeholder,
  name,
  label,
}: ReviewInputProps) {
  return (
    <div className='flex flex-col justify-center items-start gap-1 w-4/5'>
      <label htmlFor={name} className='px-2'>
        {label}:
      </label>
      <input
        type={type}
        name={name}
        id={name}
        placeholder={placeholder}
        className='border rounded-xl py-1 px-3 w-full focus:scale-102 transition duration-200'
      />
    </div>
  );
}

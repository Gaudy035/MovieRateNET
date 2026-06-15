interface ReviewTextAreaProps {
  placeholder: string;
  name: string;
  label: string;
}

export default function ReviewTextArea({
  placeholder,
  name,
  label,
}: ReviewTextAreaProps) {
  return (
    <div className='flex flex-col justify-center items-start gap-1 w-4/5'>
      <label htmlFor={name} className='px-2'>
        {label}:
      </label>
      <textarea
        name={name}
        id={name}
        rows={5}
        placeholder={placeholder}
        className='border rounded-xl py-1 px-3 resize-none w-full focus:scale-102 transition duration-200'
      />
    </div>
  );
}

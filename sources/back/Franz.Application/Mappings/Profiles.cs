using Franz.Common.Mapping;
using Franz.Contracts.DTOs;
using Franz.Domain.Entities;
using Franz.Domain.ValueObjects;

namespace Franz.Application.Mappings
{
  public class ApplicationProfile : FranzMapProfile
  {
    public ApplicationProfile()
    {
      // 🧩 Book → BookDto
      CreateMap<Book, BookDto>()
          .ForMember(d => d.Id, s => s.Id)
          .ForMember(d => d.Title, s => s.Title)
          .ForMember(d => d.Author, s => s.Author)
          .ForMember(d => d.Isbn, s => s.Isbn)
          .ForMember(d => d.PublishedOn, s => s.PublishedOn)
          .ForMember(d => d.CopiesAvailable, s => s.CopiesAvailable)
          .ConstructUsing(book => new BookDto(
              book.Id,
              book.Title,
              book.Author,
              book.Isbn,
              book.PublishedOn,
              book.CopiesAvailable
          ))
          .ReverseConstructUsing(dto => new Book(
              new ISBN(dto.Isbn),
              new Title(dto.Title),
              new Author(dto.Author),
              dto.PublishedOn,
              dto.CopiesAvailable
          ));

      // 🧠 Member → MemberDto
      CreateMap<Member, MemberDto>()
          .ForMember(d => d.Id, s => s.Id)
          .ForMember(d => d.FullName, s => s.Name)
          .ForMember(d => d.Email, s => s.Email)
          .ForMember(d => d.BorrowedBooksCount, s => s.BorrowedBooks.Count)
          .ConstructUsing(member => new MemberDto(
              member.Id,
              member.Name,
              member.Email,
              member.BorrowedBooks.Count
          ))
          .ReverseConstructUsing(dto => new Member(
              new FullName(dto.FullName),
              new Email(dto.Email)
          ));
    }
  }
}

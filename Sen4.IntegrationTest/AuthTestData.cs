using Infrastructure.DTO;

namespace Sen4.IntegrationTests;

public class AuthTestData
{
    public static TheoryData<UserRegistrationDTO> InvalidDataForRegistration => new TheoryData<UserRegistrationDTO>
    {
        //invalid email
        new UserRegistrationDTO()
        {
            Email = "invalidemail",
            Name = "ValidName",
            Surname = "ValidSurname",
            MiddleName = "ValidMiddleName",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "ValidPass1@"
        },
        //short password
        new UserRegistrationDTO()
        {
            Email = "validemail1@gmail.com",
            Name = "ValidName",
            Surname = "ValidSurname",
            MiddleName = "ValidMiddleName",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "Short1"
        },
        //password without specific symbols
        new UserRegistrationDTO()
        {
            Email = "validemail2@gmail.com",
            Name = "ValidName",
            Surname = "ValidSurname",
            MiddleName = "ValidMiddleName",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "NoSpecialChar1"
        },
        //empty name
        new UserRegistrationDTO
        {
            Email = "validemail@gmail.com",
            Name = "",
            Surname = "ValidSurname",
            MiddleName = "ValidMiddleName",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "ValidPass1@"
        },
        //null value
        new UserRegistrationDTO
        {
            Email = null,
            Name = "ValidName",
            Surname = "ValidSurname",
            MiddleName = "ValidMiddleName",
            PostId = new Guid("6266ad9b-a32b-452e-b34f-32a0cc3b1d2b"),
            Password = "ValidPass1@"
        }
    };
    public static TheoryData<LoginDTO> InvalidDataForLogin =>
        new TheoryData<LoginDTO>
        {
            //invalid email
            new LoginDTO()
            {
                Email = "alehandro",
                Password = "alehandro_Kj8_Dn3456_ty5&"
            },
            //empty email
            new LoginDTO()
            {
                Email = "",
                Password = "emmptytest_Kj8_Dn3456_ty5&"
            }
        };
}
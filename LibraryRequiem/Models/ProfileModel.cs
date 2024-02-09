namespace LibraryRequiem.Models
{
    public class ProfileModel
    {
        public int Id { get; set; }

        public LikedBookModel likedBooks { get; set; }

        public byte[] AccountIcon { get; set; }

    }
}

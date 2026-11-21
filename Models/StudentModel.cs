using System.ComponentModel.DataAnnotations;

namespace StudentManagementAPI.Models
{
    public class StudentModel
    {
        [Required(ErrorMessage = "Student ID is required")]
        [Range(1, 999999, ErrorMessage = "Enter a valid Student ID")]
        public int Sid { get; set; }

        [Required(ErrorMessage = "Student name is required")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Name must contain 3 to 50 characters")]
        public string Sname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course is required")]
        [StringLength(30, ErrorMessage = "Course cannot exceed 30 characters")]
        public string Course { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fees are required")]
        [Range(1, 1000000, ErrorMessage = "Fees must be between 1 and 1000000")]
        public decimal Fees { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]{10}$",ErrorMessage = "Mobile number must contain exactly 10 digits")]
        public string Mobile { get; set; } = string.Empty;
    }
}
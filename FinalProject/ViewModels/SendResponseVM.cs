﻿namespace FinalProject.ViewModels
{
    public class SendResponseVM
    {
        public int PostId { get; set; }
        public int FeedbackId { get; set; }
        public string Address { get; set; }
        public string FeedbackName { get; set; }
        public string ResponseText { get; set; }
        public List<ResponseImageVM> ResponseImageVMs { get; set; } = new List<ResponseImageVM>();
        public List<IFormFile> UploadedImages { get; set; } = new List<IFormFile>();
    }
}

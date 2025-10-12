namespace Pastebin.Models;
﻿
﻿public class Like
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid PasteId { get; set; }
    public Paste? Paste { get; set; }
﻿}
﻿
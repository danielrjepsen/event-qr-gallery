namespace Nory.Core.Domain.Enums;

public enum ActivityType
{
    // photo
    PhotoUploaded,
    PhotoViewed,
    PhotoDownloaded,
    PhotoLiked,
    PhotoShared,

    // app
    GuestAppOpened,
    GalleryViewed,
    SlideshowViewed,

    // event
    QrCodeScanned,
    EventJoined,
    EventLeft,

    // guestbook
    GuestbookEntryAdded,
    GuestbookViewed,

    // general
    PageViewed,
    SessionStarted,
    SessionEnded,
}

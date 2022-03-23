namespace Awesome
{
    public class MySweetController : ApiController
    {
        [HttpPut("Details/{id}")]
        public async Task<ParentDTO> UpdateMyBusinessEntity([FromRoute] long id, [FromBody] ParentDTO parentDto)
        {
            //TODO remove dbcontext
            var Parent = await _dbContext.Parent.Include(x => x.CalculationTotals).FirstOrDefaultAsync(x => x.RecordId == id);
            parentDto.ParentUploadComplete = false;
            parentDto.ParentUploadError = false;
            var instance = int.Parse(parentDto.Parentinstance);

            if (Parent == null)
            {
                throw new StatusCodeException(HttpStatusCode.NotFound);
            }

            parentDto.ToModel(Parent);

            if (parentDto.ReadyForSave)
            {
                try
                {
                    using (var session = new MyApp.Services.Session(_config["MyApp.Services:Environment"],
                        _config["MyApp.Services:Username"], _config["MyApp.Services:Password"]))
                    {
                        var Model = session.Get(instance);
                        parentDto.SetModelDifferences(Model);

                        if (parentDto.IsNew || !parentDto.HasErrors)
                        {
                            var UserLogOn =
                                await _dbContext.TblUserLogOn.FirstOrDefaultAsync(x =>
                                    x.UserId == int.Parse(Parent.CustomerRepName));

                            session.Update(instance, parentDto.ToModel(Model));
                            session.CreateEntries(
                                parentDto.CalculationTotalsDTOList.Select(x => x.ToEntryModel()),
                                parentDto.ParentreferenceNumber,
                                UserLogOn.UserId.ToString(),
                                UserLogOn.UserTypeId
                            );
                            parentDto.ParentUploadComplete = true;
                            parentDto.ParentUploadError = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(LoggingEvents.Exception, ex, ex.Message);

                    parentDto.ParentUploadComplete = false;
                    parentDto.ParentUploadError = true;
                    parentDto.UploadErrorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        parentDto.UploadErrorMessage += " " + ex.InnerException.Message;
                    }
                }

                Parent.ParentUploadComplete = parentDto.ParentUploadComplete;
                Parent.ParentUploadError = parentDto.ParentUploadError;
            }

            _dbContext.Parent.Update(Parent);
            _dbContext.Entry<Parent>(Parent).Property(x => x.Steps).IsModified = false;//don't save the thing...
            await _dbContext.SaveChangesAsync();

            // refresh the data
            parentDto.CalculationTotalsDTOList = await GetMyBusinessEntityCalculationTotals(id);

            return parentDto;
        }
    }
}

// This controller probably started off well, but kept getting larger as more business requirements got added.
// Need to separate concerns early, when they are small. (Unit testing should feel trivial, because you're only testing a small unit of work)

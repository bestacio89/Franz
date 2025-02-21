using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Franz.Application;
internal class function
{
  public IEnumerable<ProductMiseInfo> GetProductSettingBPADDCGap(int qualityEngineerId)
  {
    var appProduct = _configuration.GetSection(appProductKey).Get<IDictionary<string, string>>();
    var bpaDDCGapValue = int.Parse(appProduct[bpaDDCGapKey] as string);

    var productSettingLst = _settingRepository.ListAll()
        .Include(s => s.FollowedComplement)
            .ThenInclude(s => s.Product)
        .Include(s => s.SettingDetails)
        .Where(o =>
            o.FollowedComplement.Product.QualityEngineer == qualityEngineerId  // ✅ Removed Convert.ToInt32()
            && (o.FollowedComplement.Product.StatusId == (int)ProductStatusEnum.Actif
                || o.FollowedComplement.Product.StatusId == (int)ProductStatusEnum.Inactif)
            && o.SettingDetails.Any(s =>
                s.NumberOfSetting.Contains("PR") && s.TastingStatusId.HasValue
                && s.TastingStatusId.Value == (int)TastingStatusEnum.Agree
                && !s.AnomalyHistories.Any(u => u.SettingDetailId == s.Id
                    && u.AnomalyQualityId == (int)AnomalyQualityEnum.BPADDCGap))
            && o.SettingDetails.Any(s =>
                s.NumberOfSetting.Contains("PO") && s.TastingStatusId.HasValue
                && s.TastingStatusId.Value == (int)TastingStatusEnum.Agree
                && !s.AnomalyHistories.Any(u => u.SettingDetailId == s.Id
                    && u.AnomalyQualityId == (int)AnomalyQualityEnum.BPADDCGap)))
        .AsEnumerable()  // ✅ Forces in-memory execution for non-translatable parts
        .Where(o =>
            o.SettingDetails
                .OrderBy(s => s.TastingDate)
                .FirstOrDefault(s =>
                    s.NumberOfSetting.Contains("PO") && s.TastingStatusId.HasValue
                    && s.TastingStatusId.Value == (int)TastingStatusEnum.Agree
                    && !s.AnomalyHistories.Any(u => u.SettingDetailId == s.Id
                        && u.AnomalyQualityId == (int)AnomalyQualityEnum.BPADDCGap)
                )?.EstimatedDateOfConditioning.HasValue == true
            &&
            o.SettingDetails
                .OrderBy(s => s.TastingDate)
                .FirstOrDefault(s =>
                    s.NumberOfSetting.Contains("PR") && s.TastingStatusId.HasValue
                    && s.TastingStatusId.Value == (int)TastingStatusEnum.Agree
                    && !s.AnomalyHistories.Any(u => u.SettingDetailId == s.Id
                        && u.AnomalyQualityId == (int)AnomalyQualityEnum.BPADDCGap)
                )?.TastingDate.HasValue == true
            &&
            o.SettingDetails
                .OrderBy(s => s.TastingDate)
                .FirstOrDefault(s => s.NumberOfSetting.Contains("PO"))
                .EstimatedDateOfConditioning.Value
                .SubstractNbWorkingDays(
                    o.SettingDetails
                        .OrderBy(s => s.TastingDate)
                        .FirstOrDefault(s => s.NumberOfSetting.Contains("PR"))
                        .TastingDate.Value
                ) > bpaDDCGapValue
        )
        .OrderByDescending(o => o.SettingDetails.OrderByDescending(u => u.UpdateDate).FirstOrDefault().UpdateDate)
        .GroupBy(o => o.FollowedComplement.ProductId)
        .Select(g => new ProductMiseInfo
        {
          ID = g.Key,
          Matnr = g.FirstOrDefault()?.FollowedComplement?.Product?.Matnr ?? "",
          SettingDetailsCurrent = g.FirstOrDefault()?.SettingDetails
                .Where(s => !s.AnomalyHistories.Any(u => u.SettingDetailId == s.Id
                    && u.AnomalyQualityId == (int)AnomalyQualityEnum.BPADDCGap))
                .ToList(),
          IsSensitiveProduct = g.FirstOrDefault()?.FollowedComplement?.Product?.IsSensitiveProduct ?? false
        })
        .Take(20)
        .ToList();  // ✅ Ensure query execution in memory

    foreach (var productSetting in productSettingLst)
    {
      var settingDetail = productSetting.SettingDetailsCurrent;
      if (settingDetail != null && settingDetail.Any())
      {
        var prDetail = settingDetail.OrderBy(s => s.TastingDate)
            .FirstOrDefault(s => s.NumberOfSetting.Contains("PR") && s.TastingStatusId.HasValue
            && s.TastingStatusId.Value == (int)TastingStatusEnum.Agree);

        var poDetail = settingDetail.OrderBy(s => s.TastingDate)
            .FirstOrDefault(s => s.NumberOfSetting.Contains("PO") && s.TastingStatusId.HasValue
            && s.TastingStatusId.Value == (int)TastingStatusEnum.Agree);

        if (prDetail != null && poDetail != null && prDetail.TastingDate.HasValue && poDetail.EstimatedDateOfConditioning.HasValue)
        {
          productSetting.SettingDetailId = prDetail.Id;
          productSetting.GapDay = poDetail.EstimatedDateOfConditioning.Value
              .SubstractNbWorkingDays(prDetail.TastingDate.Value) - bpaDDCGapValue;
        }
      }
    }

    return SetProductSapFields(productSettingLst);
  }

}
